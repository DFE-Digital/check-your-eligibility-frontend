using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class RemoveChildUseCaseTests
    {
        private RemoveChildUseCase _sut;
        private Mock<ILogger<RemoveChildUseCase>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<RemoveChildUseCase>>();
            _sut = new RemoveChildUseCase(_loggerMock.Object);
        }

        [Test]
        public async Task ExecuteAsync_WithValidIndex_ShouldRemoveChild()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    new Child { FirstName = "Child1" },
                    new Child { FirstName = "Child2" }
                }
            };

            // Act
            var result = await _sut.ExecuteAsync(children, 0);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.UpdatedChildren.ChildList.Should().HaveCount(1);
            result.UpdatedChildren.ChildList.Should().NotContain(x => x.FirstName == "Child1");
            result.ErrorMessage.Should().BeEmpty();
        }

        [Test]
        public async Task ExecuteAsync_WithInvalidIndex_ShouldReturnError()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    new Child { FirstName = "Child1" }
                }
            };

            // Act
            var result = await _sut.ExecuteAsync(children, 1);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.UpdatedChildren.ChildList.Should().HaveCount(1);
            result.ErrorMessage.Should().Be("Invalid child index");
        }

        [Test]
        public async Task ExecuteAsync_WithNullRequest_ShouldReturnError()
        {
            // Act
            var result = await _sut.ExecuteAsync(null, 0);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid request - no children list available");
        }

        [Test]
        public async Task ExecuteAsync_WhenSuccessful_ShouldLogInformation()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    new Child { FirstName = "Child1" },
                    new Child { FirstName = "Child2" }
                }
            };

            // Act
            await _sut.ExecuteAsync(children, 0);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully removed child")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}