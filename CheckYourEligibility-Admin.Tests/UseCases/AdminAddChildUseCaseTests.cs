using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminAddChildUseCaseTests
    {
        private Mock<ILogger<AdminAddChildUseCase>> _loggerMock;
        private Mock<IConfiguration> _configMock;
        private AdminAddChildUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminAddChildUseCase>>();
            _configMock = new Mock<IConfiguration>();

            // Configure max children setting
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(x => x.Value).Returns("99");
            _configMock.Setup(x => x.GetSection("MaxChildren"))
                .Returns(configSection.Object);

            _sut = new AdminAddChildUseCase(_loggerMock.Object, _configMock.Object);
        }

        [Test]
        public async Task Execute_WithSpaceForNewChild_ShouldAddChildToList()
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
            var result = await _sut.Execute(children);

            // Assert
            result.ChildList.Should().HaveCount(2);
            result.ChildList.Last().Should().BeEquivalentTo(new Child());
        }

        [Test]
        public async Task Execute_WithMaximumChildren_ShouldThrowException()
        {
            // Arrange
            var children = new Children
            {
                ChildList = Enumerable.Range(1, 99)
                    .Select(_ => new Child())
                    .ToList()
            };

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(children))
                .Should().ThrowAsync<AdminAddChildException>()
                .WithMessage("Maximum limit of 99 children reached");
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
                _sut.Execute(children))
                .Should().ThrowAsync<AdminAddChildException>()
                .WithMessage("Failed to add child: Object reference not set to an instance of an object.");
        }

        [Test]
        public async Task Execute_WithNullRequest_ShouldThrowException()
        {
            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(null))
                .Should().ThrowAsync<AdminAddChildException>()
                .WithMessage("Failed to add child: Object reference not set to an instance of an object.");
        }

        [Test]
        public async Task Execute_ShouldLogSuccess()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child> { new Child() }
            };

            // Act
            await _sut.Execute(children);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Successfully added new child")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}