using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckYourEligibility_Parent.Tests.Usecases
{
    [TestFixture]
    public class EnterChildDetailsUseCaseTests
    {
        private Mock<ILogger<EnterChildDetailsUseCase>> _loggerMock;
        private EnterChildDetailsUseCase _sut;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<EnterChildDetailsUseCase>>();
            _sut = new EnterChildDetailsUseCase(_loggerMock.Object);
        }

        [Test]
        public async Task ExecuteAsync_WithNoData_ShouldReturnDefaultChildren()
        {
            // Act
            var result = await _sut.ExecuteAsync(null, null);

            // Assert
            result.Should().NotBeNull();
            result.ChildList.Should().NotBeNull();
            result.ChildList.Should().HaveCount(1);
            result.ChildList.First().Should().NotBeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithChildAddOrRemove_ShouldReturnDeserializedChildren()
        {
            // Arrange
            var childList = new List<Child>
            {
                new Child { FirstName = "Test", LastName = "Child" },
                new Child { FirstName = "Test2", LastName = "Child2" }
            };
            var childListJson = JsonConvert.SerializeObject(childList);

            // Act
            var result = await _sut.ExecuteAsync(childListJson, true);

            // Assert
            result.Should().NotBeNull();
            result.ChildList.Should().NotBeNull();
            result.ChildList.Should().HaveCount(2);
            result.ChildList.First().FirstName.Should().Be("Test");
            result.ChildList.First().LastName.Should().Be("Child");
        }

        [Test]
        public async Task ExecuteAsync_WithInvalidJson_ShouldThrowException()
        {
            // Arrange
            var invalidJson = "invalid json";

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.ExecuteAsync(invalidJson, true))
                .Should().ThrowAsync<JsonReaderException>();

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WithChildAddOrRemoveFalse_ShouldReturnDefaultChildren()
        {
            // Arrange
            var childList = new List<Child>
            {
                new Child { FirstName = "Test", LastName = "Child" }
            };
            var childListJson = JsonConvert.SerializeObject(childList);

            // Act
            var result = await _sut.ExecuteAsync(childListJson, false);

            // Assert
            result.Should().NotBeNull();
            result.ChildList.Should().NotBeNull();
            result.ChildList.Should().HaveCount(1);
            result.ChildList.First().FirstName.Should().BeNull();
            result.ChildList.First().LastName.Should().BeNull();
        }
    }
}