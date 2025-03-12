using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class LoadParentDetailsUseCaseTests
    {
        private LoadParentDetailsUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new LoadParentDetailsUseCase();
        }

        [Test]
        public async Task Execute_WithValidParentDetails_ShouldDeserializeParent()
        {
            // Arrange
            var parent = new Parent { FirstName = "John", LastName = "Doe" };
            var parentJson = JsonConvert.SerializeObject(parent);

            // Act
            var result = await _sut.Execute(parentJson);

            // Assert
            result.Parent.Should().BeEquivalentTo(parent);
        }

        [Test]
        public async Task Execute_WithInvalidParentJson_ShouldReturnNullParent()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act
            var result = await _sut.Execute(invalidJson);

            // Assert
            result.Parent.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithNullParentJson_ShouldReturnNullParent()
        {
            // Act
            var result = await _sut.Execute(null);

            // Assert
            result.Parent.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithValidValidationErrors_ShouldDeserializeErrors()
        {
            // Arrange
            var errors = new Dictionary<string, List<string>>
            {
                { "Field1", new List<string> { "Error1", "Error2" } },
                { "Field2", new List<string> { "Error3" } }
            };
            var errorsJson = JsonConvert.SerializeObject(errors);

            // Act
            var result = await _sut.Execute(validationErrorsJson: errorsJson);

            // Assert
            result.ValidationErrors.Should().BeEquivalentTo(errors);
        }

        [Test]
        public async Task Execute_WithInvalidValidationErrorsJson_ShouldReturnNullValidationErrors()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act
            var result = await _sut.Execute(validationErrorsJson: invalidJson);

            // Assert
            result.ValidationErrors.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithNullValidationErrorsJson_ShouldReturnNullValidationErrors()
        {
            // Act
            var result = await _sut.Execute(validationErrorsJson: null);

            // Assert
            result.ValidationErrors.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithNASSValidationError_ShouldRemoveNASSValidationError()
        {
            // Arrange
            var errors = new Dictionary<string, List<string>>
            {
                { "Field1", new List<string> { "Error1" } },
                { "NationalAsylumSeekerServiceNumber", new List<string> { "NASS Error" } }
            };
            var errorsJson = JsonConvert.SerializeObject(errors);

            // Act
            var result = await _sut.Execute(validationErrorsJson: errorsJson);

            // Assert
            result.ValidationErrors.Should().NotContainKey("NationalAsylumSeekerServiceNumber");
            result.ValidationErrors.Should().ContainKey("Field1");
        }

        [Test]
        public async Task Execute_WithValidParentAndErrors_ShouldDeserializeBoth()
        {
            // Arrange
            var parent = new Parent { FirstName = "John", LastName = "Doe" };
            var parentJson = JsonConvert.SerializeObject(parent);

            var errors = new Dictionary<string, List<string>>
            {
                { "Field1", new List<string> { "Error1" } }
            };
            var errorsJson = JsonConvert.SerializeObject(errors);

            // Act
            var result = await _sut.Execute(parentJson, errorsJson);

            // Assert
            result.Parent.Should().BeEquivalentTo(parent);
            result.ValidationErrors.Should().BeEquivalentTo(errors);
        }
    }
}
