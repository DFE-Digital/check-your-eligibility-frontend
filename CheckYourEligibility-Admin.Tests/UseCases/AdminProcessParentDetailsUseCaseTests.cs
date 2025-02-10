using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminProcessParentDetailsUseCaseTests
    {
        private Mock<ILogger<AdminProcessParentDetailsUseCase>> _loggerMock;
        private Mock<IEcsCheckService> _checkServiceMock;
        private Mock<ISession> _sessionMock;
        private AdminProcessParentDetailsUseCase _sut;
        private ParentGuardian _testParent;
        private Dictionary<string, byte[]> _sessionStorage;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminProcessParentDetailsUseCase>>();
            _checkServiceMock = new Mock<IEcsCheckService>();
            _sessionMock = new Mock<ISession>();
            _sessionStorage = new Dictionary<string, byte[]>();

            SetupSessionMock();

            _sut = new AdminProcessParentDetailsUseCase(
                _loggerMock.Object,
                _checkServiceMock.Object);

            _testParent = new ParentGuardian
            {
                FirstName = "John",
                LastName = "Doe",
                Day = "01",
                Month = "01",
                Year = "1990",
                EmailAddress = "john.doe@test.com",
                NinAsrSelection = ParentGuardian.NinAsrSelect.NinSelected,
                NationalInsuranceNumber = "AB123456C"
            };
        }

        private void SetupSessionMock()
        {
            _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) => _sessionStorage[key] = value);

            _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) =>
                {
                    var result = _sessionStorage.TryGetValue(key, out var storedValue);
                    value = storedValue;
                    return result;
                });

            _sessionMock.Setup(s => s.Remove(It.IsAny<string>()))
                .Callback<string>(key => _sessionStorage.Remove(key));
        }

        [Test]
        public async Task Execute_WithNinoSelected_ShouldProcessSuccessfully()
        {
            // Arrange
            var expectedResponse = new CheckEligibilityResponse
            {
                Data = new StatusValue { Status = "eligible" }
            };

            _checkServiceMock.Setup(x => x.PostCheck(It.IsAny<CheckEligibilityRequest_Fsm>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.Execute(_testParent, _sessionMock.Object);

            // Assert
            result.Response.Should().BeEquivalentTo(expectedResponse);
            result.RedirectAction.Should().Be("Loader");

            // Verify session data
            Encoding.UTF8.GetString(_sessionStorage["ParentFirstName"]).Should().Be("John");
            Encoding.UTF8.GetString(_sessionStorage["ParentLastName"]).Should().Be("Doe");
            Encoding.UTF8.GetString(_sessionStorage["ParentDOB"]).Should().Be("1990-01-01");
            Encoding.UTF8.GetString(_sessionStorage["ParentNINO"]).Should().Be("AB123456C");
            _sessionStorage.Should().NotContainKey("ParentNASS");
        }

        [Test]
        public async Task Execute_WithNassSelected_ShouldProcessSuccessfully()
        {
            // Arrange
            _testParent.NinAsrSelection = ParentGuardian.NinAsrSelect.AsrnSelected;
            _testParent.NationalInsuranceNumber = null;
            _testParent.NationalAsylumSeekerServiceNumber = "12345678";

            var expectedResponse = new CheckEligibilityResponse
            {
                Data = new StatusValue { Status = "eligible" }
            };

            _checkServiceMock.Setup(x => x.PostCheck(It.IsAny<CheckEligibilityRequest_Fsm>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.Execute(_testParent, _sessionMock.Object);

            // Assert
            result.Response.Should().BeEquivalentTo(expectedResponse);
            result.RedirectAction.Should().Be("Loader");

            // Verify session data
            Encoding.UTF8.GetString(_sessionStorage["ParentNASS"]).Should().Be("12345678");
            _sessionStorage.Should().NotContainKey("ParentNINO");
        }

        [Test]
        public async Task Execute_WithNoSelection_ShouldThrowException()
        {
            // Arrange
            _testParent.NinAsrSelection = ParentGuardian.NinAsrSelect.None;

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(_testParent, _sessionMock.Object))
                .Should().ThrowAsync<AdminProcessParentDetailsException>()
                .WithMessage("*Please select one option*");
        }

        [Test]
        public async Task Execute_WhenCheckServiceFails_ShouldThrowException()
        {
            // Arrange
            _checkServiceMock.Setup(x => x.PostCheck(It.IsAny<CheckEligibilityRequest_Fsm>()))
                .ThrowsAsync(new Exception("API Error"));

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(_testParent, _sessionMock.Object))
                .Should().ThrowAsync<AdminProcessParentDetailsException>()
                .WithMessage("Failed to process eligibility check: API Error");
        }
    }
}