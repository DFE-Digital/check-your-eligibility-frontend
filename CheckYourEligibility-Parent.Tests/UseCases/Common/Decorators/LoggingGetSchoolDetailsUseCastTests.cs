using CheckYourEligibility_FrontEnd.Domain.Schools;
using CheckYourEligibility_FrontEnd.UseCases.Common.Decorators;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using NUnit.Framework;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.UseCases.Schools.GetSchoolDetailsUseCase;

namespace CheckYourEligibility_Parent.Tests.UseCases.Common.Decorators
{
    [TestFixture]
    public class LoggingGetSchoolDetailsUseCaseTests
    {
        private Mock<IGetSchoolDetailsUseCase> _innerUseCaseMock;
        private Mock<ILogger<LoggingGetSchoolDetailsUseCase>> _loggerMock;
        private LoggingGetSchoolDetailsUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _innerUseCaseMock = new Mock<IGetSchoolDetailsUseCase>();
            _loggerMock = new Mock<ILogger<LoggingGetSchoolDetailsUseCase>>();
            _sut = new LoggingGetSchoolDetailsUseCase(_innerUseCaseMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task ExecuteAsync_WhenSuccessful_ShouldPassThroughResult()
        {
            // Arrange
            var request = new GetSchoolDetailsRequest("Test School");
            var expectedResponse = GetSchoolDetailsResponse.Success(new List<Establishment>());
            _innerUseCaseMock.Setup(x => x.ExecuteAsync(request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.ExecuteAsync(request);

            // Assert
            result.Should().Be(expectedResponse);
            _innerUseCaseMock.Verify(x => x.ExecuteAsync(request), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WhenExceptionOccurs_ShouldLogErrorAndRethrow()
        {
            // Arrange
            var request = new GetSchoolDetailsRequest("Test School");
            var expectedException = new Exception("Test exception");
            _innerUseCaseMock.Setup(x => x.ExecuteAsync(request)).ThrowsAsync(expectedException);

            // Act & Assert
            var act = () => _sut.ExecuteAsync(request);
            await act.Should().ThrowAsync<Exception>().Where(ex => ex == expectedException);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error searching schools with query")),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public void Constructor_WhenInnerUseCaseIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var act = () => new LoggingGetSchoolDetailsUseCase(null, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("inner");
        }

        [Test]
        public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var act = () => new LoggingGetSchoolDetailsUseCase(_innerUseCaseMock.Object, null);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }
    }
}