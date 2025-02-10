using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminChangeChildDetailsUseCaseTests
    {
        private AdminChangeChildDetailsUseCase _sut;
        private Mock<ILogger<AdminChangeChildDetailsUseCase>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminChangeChildDetailsUseCase>>();
            _sut = new AdminChangeChildDetailsUseCase(_loggerMock.Object);
        }

        [Test]
        public async Task Execute_WithValidJson_ShouldReturnChildren()
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
            var result = await _sut.Execute(json);

            // Assert
            result.Should().BeEquivalentTo(children);
        }

        [Test]
        public async Task Execute_WithNullJson_ShouldThrowException()
        {
            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(null))
                .Should().ThrowAsync<AdminChangeChildDetailsException>()
                .WithMessage("FSM Application JSON is null or empty");
        }

        [Test]
        public async Task Execute_WithInvalidJson_ShouldThrowException()
        {
            // Arrange
            var invalidJson = "invalid json";

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(invalidJson))
                .Should().ThrowAsync<AdminChangeChildDetailsException>()
                .WithMessage("Failed to parse application data");
        }

        [Test]
        public async Task Execute_WithMissingChildren_ShouldThrowException()
        {
            // Arrange
            var application = new FsmApplication { Children = null };
            var json = JsonConvert.SerializeObject(application);

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(json))
                .Should().ThrowAsync<AdminChangeChildDetailsException>()
                .WithMessage("Failed to deserialize FSM Application or Children is null");
        }
    }
}