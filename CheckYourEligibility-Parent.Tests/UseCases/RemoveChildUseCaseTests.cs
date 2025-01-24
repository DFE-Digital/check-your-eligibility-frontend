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
            result.ChildList.Should().HaveCount(1);
            result.ChildList.Should().NotContain(x => x.FirstName == "Child1");
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
            await FluentActions.Invoking(() =>_sut.ExecuteAsync(children, 1))
                .Should().ThrowAsync<RemoveChildValidationException>("Invalid child index");
        }

        [Test]
        public async Task ExecuteAsync_WithNullRequest_ShouldReturnError()
        {
            // Act
            await FluentActions.Invoking(() =>_sut.ExecuteAsync(null, 0))
                .Should().ThrowAsync<RemoveChildValidationException>("Invalid request - no children list available");
        }
    }
}