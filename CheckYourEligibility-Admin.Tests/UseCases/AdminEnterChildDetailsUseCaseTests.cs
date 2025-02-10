using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminEnterChildDetailsUseCaseTests
    {
        private Mock<ILogger<AdminEnterChildDetailsUseCase>> _loggerMock;
        private AdminEnterChildDetailsUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminEnterChildDetailsUseCase>>();
            _sut = new AdminEnterChildDetailsUseCase(_loggerMock.Object);
        }

        [Test]
        public async Task Execute_WithNoData_ShouldReturnDefaultChildren()
        {
            // Act
            var result = await _sut.Execute(null, null);

            // Assert 
            result.Should().NotBeNull();
            result.ChildList.Should().NotBeNull();
            result.ChildList.Should().HaveCount(1);
            result.ChildList.First().Should().NotBeNull();
        }

        [Test]
        public async Task Execute_WithChildAddOrRemove_ShouldReturnDeserializedChildren()
        {
            // Arrange
            var childList = new List<Child>
            {
                new Child { FirstName = "Test", LastName = "Child" },
                new Child { FirstName = "Test2", LastName = "Child2" }
            };
            var childListJson = JsonConvert.SerializeObject(childList);

            // Act
            var result = await _sut.Execute(childListJson, true);

            // Assert
            result.Should().NotBeNull();
            result.ChildList.Should().NotBeNull();
            result.ChildList.Should().HaveCount(2);
            result.ChildList.First().FirstName.Should().Be("Test");
            result.ChildList.First().LastName.Should().Be("Child");
        }

        [Test]
        public async Task Execute_WithInvalidJson_ShouldThrowException()
        {
            // Arrange
            var invalidJson = "invalid json";

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(invalidJson, true))
                .Should().ThrowAsync<AdminEnterChildDetailsException>()
                .WithMessage("Failed to enter child details: Failed to load existing children");
        }

        [Test]
        public async Task Execute_WithNoChildAddOrRemove_ShouldReturnDefaultList()
        {
            // Arrange
            var childList = new List<Child>
            {
                new Child { FirstName = "Test", LastName = "Child" }
            };
            var childListJson = JsonConvert.SerializeObject(childList);

            // Act
            var result = await _sut.Execute(childListJson, false);

            // Assert
            result.Should().NotBeNull();
            result.ChildList.Should().NotBeNull();
            result.ChildList.Should().HaveCount(1);
            result.ChildList.First().Should().BeEquivalentTo(new Child());
        }
    }
}