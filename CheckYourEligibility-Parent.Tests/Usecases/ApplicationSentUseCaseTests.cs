using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class ApplicationSentUseCaseTests
    {
        private ApplicationSentUseCase _sut;
        private Mock<ILogger<ApplicationSentUseCase>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<ApplicationSentUseCase>>();
            _sut = new ApplicationSentUseCase(_loggerMock.Object);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReturnCorrectViewAndNullModel()
        {
            // Act
            var result = await _sut.ExecuteAsync();

            // Assert
            result.ViewName.Should().Be("Application_Sent");
            result.Model.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_ShouldLogInformation()
        {
            // Act
            await _sut.ExecuteAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Displaying application sent confirmation page")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}