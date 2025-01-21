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
                _checkServiceMock.Object,
                _sessionMock.Object
            );
        }

        public static object[] StatusTestCases = {
            new object[] { "eligible", "Outcome/Eligible", "/check/signIn" },
            new object[] { "notEligible", "Outcome/Not_Eligible", (string)null },
            new object[] { "parentNotFound", "Outcome/Not_Found", (string)null },
            new object[] { "DwpError", "Outcome/Technical_Error", (string)null },
            new object[] { "queuedForProcessing", "Loader", (string)null }
        };

        [TestCaseSource(nameof(StatusTestCases))]
        public async Task ExecuteAsync_WithValidStatus_ShouldReturnCorrectViewAndModel(
            string status,
            string expectedView,
            string expectedModel)
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
            var (viewName, model) = await _sut.ExecuteAsync(responseJson);

            // Assert
            viewName.Should().Be(expectedView);
            model.Should().Be(expectedModel);
            _sessionMock.Verify(s =>
                s.Set("CheckResult", It.Is<byte[]>(b =>
                    Encoding.UTF8.GetString(b) == status)),
                Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WithEmptyResponse_ShouldReturnTechnicalError()
        {
            // Act
            var (viewName, model) = await _sut.ExecuteAsync(null);

            // Assert
            viewName.Should().Be("Outcome/Technical_Error");
            model.Should().BeNull();
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
            var (viewName, model) = await _sut.ExecuteAsync(responseJson);

            // Assert
            viewName.Should().Be("Outcome/Technical_Error");
            model.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WhenExceptionOccurs_ShouldReturnTechnicalError()
        {
            // Arrange
            var response = new CheckEligibilityResponse
            {
                Data = new StatusValue { Status = "any" }
            };
            var responseJson = JsonConvert.SerializeObject(response);

            _checkServiceMock
                .Setup(x => x.GetStatus(It.IsAny<CheckEligibilityResponse>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var (viewName, model) = await _sut.ExecuteAsync(responseJson);

            // Assert
            viewName.Should().Be("Outcome/Technical_Error");
            model.Should().BeNull();

            // Verify that the error was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}