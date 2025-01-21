using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Text;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class ProcessParentDetailsUseCaseTests
    {
        private ProcessParentDetailsUseCase _sut;
        private Mock<IEcsCheckService> _checkServiceMock;
        private Mock<ISession> _sessionMock;

        private Parent _parent;
        private CheckEligibilityResponse _eligibilityResponse;

        [SetUp]
        public void SetUp()
        {
            _checkServiceMock = new Mock<IEcsCheckService>();
            _sut = new ProcessParentDetailsUseCase(_checkServiceMock.Object);

            _sessionMock = new Mock<ISession>();
            var sessionStorage = new Dictionary<string, byte[]>();

            _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                        .Callback<string, byte[]>((key, value) => sessionStorage[key] = value);

            _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                        .Returns((string key, out byte[] value) =>
                        {
                            var result = sessionStorage.TryGetValue(key, out var storedValue);
                            value = storedValue;
                            return result;
                        });

            _parent = new Parent
            {
                FirstName = "John",
                LastName = "Doe",
                Day = "01",
                Month = "01",
                Year = "1980",
                NationalInsuranceNumber = "AB123456C",
                IsNinoSelected = true
            };

            _eligibilityResponse = new CheckEligibilityResponse
            {
                Data = new StatusValue { Status = "queuedForProcessing" },
                Links = new CheckEligibilityResponseLinks { Get_EligibilityCheck = "test-link" }
            };
        }

        [Test]
        public async Task ExecuteAsync_WithValidParent_ShouldReturnValidResponse()
        {
            // Arrange
            _checkServiceMock.Setup(s => s.PostCheck(It.IsAny<CheckEligibilityRequest_Fsm>()))
                             .ReturnsAsync(_eligibilityResponse);

            // Act
            var (isValid, response, redirectAction) = await _sut.ExecuteAsync(_parent, _sessionMock.Object);

            // Assert
            isValid.Should().BeTrue();
            response.Should().BeEquivalentTo(_eligibilityResponse);
            redirectAction.Should().Be("Loader");

            Encoding.UTF8.GetString(_sessionMock.Object.Get("ParentFirstName")).Should().Be("John");
            Encoding.UTF8.GetString(_sessionMock.Object.Get("ParentLastName")).Should().Be("Doe");
        }

        [Test]
        public async Task ExecuteAsync_WithInvalidParent_ShouldRedirectToNass()
        {
            // Arrange
            _parent.IsNinoSelected = false;
            _parent.NationalInsuranceNumber = null;

            // Act
            var (isValid, response, redirectAction) = await _sut.ExecuteAsync(_parent, _sessionMock.Object);

            // Assert
            isValid.Should().BeFalse();
            response.Should().BeNull();
            redirectAction.Should().Be("Nass");
        }

        [Test]
        public async Task ExecuteAsync_WhenApiThrowsException_ShouldThrow()
        {
            // Arrange
            _checkServiceMock.Setup(s => s.PostCheck(It.IsAny<CheckEligibilityRequest_Fsm>()))
                             .ThrowsAsync(new Exception("API Error"));

            // Act
            Func<Task> act = async () => await _sut.ExecuteAsync(_parent, _sessionMock.Object);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("API Error");
        }
    }
}
