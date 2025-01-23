using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CheckYourEligibility_Parent.Tests.Usecases
{
    [TestFixture]
    public class LoaderUseCaseTests
    {
        private Mock<ILogger<LoaderUseCase>> _loggerMock;
        private Mock<IEcsCheckService> _checkServiceMock;
        private Mock<ISession> _sessionMock;
        private LoaderUseCase _sut;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<LoaderUseCase>>();
            _checkServiceMock = new Mock<IEcsCheckService>();
            _sessionMock = new Mock<ISession>();
            _sut = new LoaderUseCase(
                _loggerMock.Object,
                _checkServiceMock.Object
            );
        }

        public static object[] StatusTestCases = {
            new object[] { "eligible", "eligible" },
            new object[] { "notEligible", "notEligible" },
            new object[] { "parentNotFound", "parentNotFound" },
            new object[] { "DwpError", "DwpError" },
            new object[] { "queuedForProcessing", "queuedForProcessing" },
        };

        [TestCaseSource(nameof(StatusTestCases))]
        public async Task ExecuteAsync_WithValidStatus_ShouldReturnCorrectViewAndModel(
            string status,
            string expectedOutcome)
        {
            // Arrange
            var response = new CheckEligibilityResponse
            {
                Data = new StatusValue { Status = status }
            };
            var responseJson = JsonConvert.SerializeObject(response);
            var statusResponse = new CheckEligibilityStatusResponse
            {
                Data = new StatusValue { Status = status }
            };

            _checkServiceMock
                .Setup(x => x.GetStatus(It.IsAny<CheckEligibilityResponse>()))
                .ReturnsAsync(statusResponse);

            // Act
            string outcome = await _sut.ExecuteAsync(responseJson, _sessionMock.Object);

            // Assert
            outcome.Should().Be(expectedOutcome);
            _sessionMock.Verify(s =>
                s.Set("CheckResult", It.Is<byte[]>(b =>
                    Encoding.UTF8.GetString(b) == status)),
                Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WithEmptyResponse_ShouldReturnTechnicalError()
        {
            // Act
            await FluentActions.Invoking(() =>
                    _sut.ExecuteAsync(
                        null, _sessionMock.Object))
                .Should().ThrowAsync<Exception>()
                .WithMessage("No response data found in TempData.");
        }

        [Test]
        public async Task ExecuteAsync_WithNullCheckResponse_ShouldReturnTechnicalError()
        {
            // Arrange
            var response = new CheckEligibilityResponse
            {
                Data = new StatusValue { Status = "any" }
            };
            var responseJson = JsonConvert.SerializeObject(response);

            _checkServiceMock
                .Setup(x => x.GetStatus(It.IsAny<CheckEligibilityResponse>()))
                .ReturnsAsync((CheckEligibilityStatusResponse)null);

            // Act
            await FluentActions.Invoking(() =>
                    _sut.ExecuteAsync(
                        responseJson, _sessionMock.Object))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Null response received from GetStatus.");
        }
    }
}