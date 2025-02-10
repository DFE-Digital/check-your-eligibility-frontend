using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminRemoveChildUseCaseTests
    {
        private Mock<ILogger<AdminRemoveChildUseCase>> _loggerMock;
        private AdminRemoveChildUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminRemoveChildUseCase>>();
            _sut = new AdminRemoveChildUseCase(_loggerMock.Object);
        }

        [Test]
        public async Task Execute_WithValidIndex_ShouldRemoveChild()
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
            var result = await _sut.Execute(children, 0);

            // Assert
            result.ChildList.Should().HaveCount(1);
            result.ChildList.Should().NotContain(x => x.FirstName == "Child1");
            result.ChildList.First().FirstName.Should().Be("Child2");
        }

        [Test]
        public async Task Execute_WithInvalidIndex_ShouldThrowException()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    new Child { FirstName = "Child1" }
                }
            };

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(children, 1))
                .Should().ThrowAsync<AdminRemoveChildException>()
                .WithMessage("Invalid child index: 1");
        }

        [Test]
        public async Task Execute_WithNegativeIndex_ShouldThrowException()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    new Child { FirstName = "Child1" }
                }
            };

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(children, -1))
                .Should().ThrowAsync<AdminRemoveChildException>()
                .WithMessage("Invalid child index: -1");
        }

        [Test]
        public async Task Execute_WithNullChildList_ShouldThrowException()
        {
            // Arrange
            var children = new Children
            {
                ChildList = null
            };

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(children, 0))
                .Should().ThrowAsync<AdminRemoveChildException>()
                .WithMessage("Invalid request - no children list available");
        }

        [Test]
        public async Task Execute_WithNullRequest_ShouldThrowException()
        {
            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(null, 0))
                .Should().ThrowAsync<AdminRemoveChildException>()
                .WithMessage("Invalid request - no children list available");
        }

        [Test]
        public async Task Execute_ShouldLogSuccess()
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
            await _sut.Execute(children, 0);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Successfully removed child")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}