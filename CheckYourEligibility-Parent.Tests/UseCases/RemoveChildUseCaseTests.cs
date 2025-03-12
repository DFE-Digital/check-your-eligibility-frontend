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
            var result = _sut.Execute(children, 0);

            // Assert
            result.ChildList.Should().HaveCount(1);
            result.ChildList.Should().NotContain(x => x.FirstName == "Child1");
        }

        [Test]
        public async Task Execute_WithInvalidIndex_ShouldReturnError()
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
            FluentActions.Invoking(() =>_sut.Execute(children, 1))
                .Should().Throw<RemoveChildValidationException>("Invalid child index");
        }

        [Test]
        public async Task Execute_WithNullRequest_ShouldReturnError()
        {
            // Act
            FluentActions.Invoking(() =>_sut.Execute(null, 0))
                .Should().Throw<RemoveChildValidationException>("Invalid request - no children list available");
        }
    }
}