using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class AddChildUseCaseTests
    {
        private AddChildUseCase _sut;
        private Mock<ILogger<AddChildUseCase>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AddChildUseCase>>();
            _sut = new AddChildUseCase(_loggerMock.Object);
        }

        [Test]
        public async Task ExecuteAsync_WithSpaceForNewChild_ShouldAddChildToList()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    new Child { FirstName = "Existing" }
                }
            };

            // Act
            var result = await _sut.ExecuteAsync(children);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.UpdatedChildren.ChildList.Should().HaveCount(2);
            result.UpdatedChildren.ChildList.Last().Should().BeEquivalentTo(new Child());
        }

        [Test]
        public async Task ExecuteAsync_WithMaximumChildren_ShouldNotAddChild()
        {
            // Arrange
            var children = new Children
            {
                ChildList = Enumerable.Range(1, 99)
                    .Select(_ => new Child())
                    .ToList()
            };

            // Act
            var result = await _sut.ExecuteAsync(children);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.UpdatedChildren.ChildList.Should().HaveCount(99);
        }

        [Test]
        public async Task ExecuteAsync_WhenSuccessful_ShouldLogInformation()
        {
            // Arrange
            var children = new Children { ChildList = new List<Child>() };

            // Act
            await _sut.ExecuteAsync(children);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully added new child")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}