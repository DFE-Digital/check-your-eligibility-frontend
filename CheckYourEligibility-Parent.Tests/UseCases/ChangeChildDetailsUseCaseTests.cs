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
            var result = _sut.Execute(json);

            // Assert
            result.Should().BeEquivalentTo(children);
        }

        [Test]
        public async Task Execute_WithNullJson_ShouldReturnEmptyChildren()
        {
            // Act
            FluentActions.Invoking(() =>
                    _sut.Execute(null))
                .Should().Throw<Exception>()
                .WithMessage("FSM Application JSON is null or empty");;
        }


        [Test]
        public async Task Execute_WithInvalidJson_ShouldThrowException()
        {
            // Arrange
            var invalidJson = "invalid json";

            // Act & Assert
            FluentActions.Invoking(() =>
                _sut.Execute(invalidJson))
                .Should().Throw<JsonReaderException>();
        }
    }
}