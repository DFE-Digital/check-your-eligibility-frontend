using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class ChangeChildDetailsUseCaseTests
    {
        private ChangeChildDetailsUseCase _sut;
        private Mock<ILogger<ChangeChildDetailsUseCase>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<ChangeChildDetailsUseCase>>();
            _sut = new ChangeChildDetailsUseCase(_loggerMock.Object);
        }

        [Test]
        public async Task ExecuteAsync_WithValidJson_ShouldReturnChildren()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    new Child { FirstName = "Test", LastName = "Child" }
                }
            };
            var application = new FsmApplication { Children = children };
            var json = JsonConvert.SerializeObject(application);

            // Act
            var result = await _sut.ExecuteAsync(json);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ViewName.Should().Be("Enter_Child_Details");
            result.Model.Should().BeEquivalentTo(children);
        }

        [Test]
        public async Task ExecuteAsync_WithNullJson_ShouldReturnEmptyChildren()
        {
            // Act
            var result = await _sut.ExecuteAsync(null);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ViewName.Should().Be("Enter_Child_Details");
            result.Model.Should().NotBeNull();

            // Treat null as empty
            var childList = result.Model.ChildList ?? new List<Child>();
            childList.Should().BeEmpty("because null ChildList is treated as no children");
        }


        [Test]
        public async Task ExecuteAsync_WithInvalidJson_ShouldThrowException()
        {
            // Arrange
            var invalidJson = "invalid json";

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.ExecuteAsync(invalidJson))
                .Should().ThrowAsync<JsonReaderException>();
        }

        [Test]
        public async Task ExecuteAsync_ShouldLogInformationOnSuccess()
        {
            // Arrange
            var children = new Children { ChildList = new List<Child>() };
            var application = new FsmApplication { Children = children };
            var json = JsonConvert.SerializeObject(application);

            // Act
            await _sut.ExecuteAsync(json);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully retrieved children details")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}