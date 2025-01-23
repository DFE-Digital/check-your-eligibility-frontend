using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class LoadParentNassDetailsUseCaseTests
    {
        private LoadParentNassDetailsUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new LoadParentNassDetailsUseCase();
        }

        [Test]
        public async Task ExecuteAsync_WithValidParentDetails_ShouldReturnDeserializedParent()
        {
            // Arrange
            var parent = new Parent { FirstName = "John", LastName = "Doe" };
            var parentJson = JsonConvert.SerializeObject(parent);

            // Act
            var result = await _sut.ExecuteAsync(parentJson);

            // Assert
            result.Should().BeEquivalentTo(parent);
        }

        [Test]
        public async Task ExecuteAsync_WithInvalidJson_ShouldReturnEmptyParent()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act
            var result = await _sut.ExecuteAsync(invalidJson);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Parent>();
        }

        [Test]
        public async Task ExecuteAsync_WithNullJson_ShouldReturnNull()
        {
            // Act
            var result = await _sut.ExecuteAsync(null);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithEmptyJson_ShouldReturnEmptyParent()
        {
            // Arrange
            var emptyJson = "{}";

            // Act
            var result = await _sut.ExecuteAsync(emptyJson);

            // Assert
            result.Should().BeEquivalentTo(new Parent());
        }
    }
}
