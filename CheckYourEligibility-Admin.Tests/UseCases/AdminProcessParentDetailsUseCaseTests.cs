using AutoFixture;
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

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class AdminProcessParentDetailsUseCaseTests
    {
        private AdminProcessParentDetailsUseCase _sut;
        private Mock<ILogger<AdminProcessParentDetailsUseCase>> _loggerMock;
        private Mock<IEcsCheckService> _checkServiceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminProcessParentDetailsUseCase>>();
            _checkServiceMock = new Mock<IEcsCheckService>();
            _sut = new AdminProcessParentDetailsUseCase(_loggerMock.Object, _checkServiceMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Execute_Should_Process_Parent_Details_And_Store_In_Session()
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.Day = "01";
            request.Month = "01";
            request.Year = "1990";

            var session = new Mock<ISession>();
            var sessionDict = new Dictionary<string, string>();

            session.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) =>
                    sessionDict[key] = System.Text.Encoding.UTF8.GetString(value));

            var expectedResponse = _fixture.Create<CheckEligibilityResponse>();

            _checkServiceMock.Setup(x => x.PostCheck(It.IsAny<CheckEligibilityRequest_Fsm>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.Execute(request, session.Object);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
            _checkServiceMock.Verify(x => x.PostCheck(It.IsAny<CheckEligibilityRequest_Fsm>()), Times.Once);

            // Verify session storage
            session.Verify(s => s.Set(
                It.Is<string>(key => key == "ParentFirstName"),
                It.Is<byte[]>(value => System.Text.Encoding.UTF8.GetString(value) == request.FirstName)
            ), Times.Once);

            session.Verify(s => s.Set(
                It.Is<string>(key => key == "ParentLastName"),
                It.Is<byte[]>(value => System.Text.Encoding.UTF8.GetString(value) == request.LastName)
            ), Times.Once);

            session.Verify(s => s.Set(
                It.Is<string>(key => key == "ParentDOB"),
                It.Is<byte[]>(value => System.Text.Encoding.UTF8.GetString(value) == "1990-01-01")
            ), Times.Once);

            session.Verify(s => s.Set(
                It.Is<string>(key => key == "ParentEmail"),
                It.Is<byte[]>(value => System.Text.Encoding.UTF8.GetString(value) == request.EmailAddress)
            ), Times.Once);
        }

        [Test]
        public async Task Execute_Should_Handle_NASS_When_Selected()
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.Day = "01";
            request.Month = "01";
            request.Year = "1990";
            request.NinAsrSelection = ParentGuardian.NinAsrSelect.AsrnSelected;
            request.NationalAsylumSeekerServiceNumber = "12345678";

            var session = new Mock<ISession>();
            var expectedResponse = _fixture.Create<CheckEligibilityResponse>();

            _checkServiceMock.Setup(x => x.PostCheck(It.IsAny<CheckEligibilityRequest_Fsm>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.Execute(request, session.Object);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);

            session.Verify(s => s.Set(
                It.Is<string>(key => key == "ParentNASS"),
                It.Is<byte[]>(value => System.Text.Encoding.UTF8.GetString(value) == request.NationalAsylumSeekerServiceNumber)
            ), Times.Once);

            session.Verify(s => s.Remove("ParentNINO"), Times.Once);
        }

        [Test]
        public async Task Execute_Should_Handle_NINO_When_Selected()
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.Day = "01";
            request.Month = "01";
            request.Year = "1990";
            request.NinAsrSelection = ParentGuardian.NinAsrSelect.NinSelected;
            request.NationalInsuranceNumber = "AB123456C";

            var session = new Mock<ISession>();
            var expectedResponse = _fixture.Create<CheckEligibilityResponse>();

            _checkServiceMock.Setup(x => x.PostCheck(It.IsAny<CheckEligibilityRequest_Fsm>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.Execute(request, session.Object);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);

            session.Verify(s => s.Set(
                It.Is<string>(key => key == "ParentNINO"),
                It.Is<byte[]>(value => System.Text.Encoding.UTF8.GetString(value) == request.NationalInsuranceNumber)
            ), Times.Once);

            session.Verify(s => s.Remove("ParentNASS"), Times.Once);
        }

        [Test]
        public void Execute_Should_Throw_Exception_When_Service_Fails()
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.Day = "01";
            request.Month = "01";
            request.Year = "1990";

            var session = new Mock<ISession>();
            _checkServiceMock.Setup(x => x.PostCheck(It.IsAny<CheckEligibilityRequest_Fsm>()))
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            Func<Task> act = async () => await _sut.Execute(request, session.Object);
            act.Should().ThrowAsync<Exception>().WithMessage("Service error");
        }
    }
}