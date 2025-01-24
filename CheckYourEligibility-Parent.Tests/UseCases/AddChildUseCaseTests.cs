using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
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
            //Arrange
            var inMemorySettings = new Dictionary<string, string> {
                {"MaxChildren", "99"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            
            _sut = new AddChildUseCase(_loggerMock.Object, configuration);
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
            result.ChildList.Should().HaveCount(2);
            result.ChildList.Last().Should().BeEquivalentTo(new Child());
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
            await FluentActions.Invoking(() =>_sut.ExecuteAsync(children))
                .Should().ThrowAsync<MaxChildrenException>();
        }
    }
}