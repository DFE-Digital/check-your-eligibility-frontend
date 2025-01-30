using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CheckYourEligibility_Parent.Tests.UseCases.Admin
{
    [TestFixture]
    public class AdminLoadParentDetailsUseCaseTests
    {
        private AdminLoadParentDetailsUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new AdminLoadParentDetailsUseCase();
        }

        [Test]
        public async Task ExecuteAsync_WithValidParentDetails_ShouldDeserializeParent()
        {
            // Arrange
            var parent = new ParentGuardian
            {
                FirstName = "John",
                LastName = "Doe",
                NinAsrSelection = ParentGuardian.NinAsrSelect.NinSelected
            };
            var parentJson = JsonConvert.SerializeObject(parent);

            // Act
            var result = await _sut.ExecuteAsync(parentJson);

            // Assert
            result.Parent.Should().BeEquivalentTo(parent);
        }

        [Test]
        public async Task ExecuteAsync_WithInvalidParentJson_ShouldReturnNullParent()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act
            var result = await _sut.ExecuteAsync(invalidJson);

            // Assert
            result.Parent.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithNullParentJson_ShouldReturnNullParent()
        {
            // Act
            var result = await _sut.ExecuteAsync(null);

            // Assert
            result.Parent.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithValidValidationErrors_ShouldDeserializeErrors()
        {
            // Arrange
            var errors = new Dictionary<string, List<string>>
            {
                { "Field1", new List<string> { "Error1", "Error2" } },
                { "Field2", new List<string> { "Error3" } }
            };
            var errorsJson = JsonConvert.SerializeObject(errors);

            // Act
            var result = await _sut.ExecuteAsync(validationErrorsJson: errorsJson);

            // Assert
            result.ValidationErrors.Should().BeEquivalentTo(errors);
        }

        [Test]
        public async Task ExecuteAsync_WithInvalidValidationErrorsJson_ShouldReturnNullValidationErrors()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act
            var result = await _sut.ExecuteAsync(validationErrorsJson: invalidJson);

            // Assert
            result.ValidationErrors.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithNullValidationErrorsJson_ShouldReturnNullValidationErrors()
        {
            // Act
            var result = await _sut.ExecuteAsync(validationErrorsJson: null);

            // Assert
            result.ValidationErrors.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithNINandNASSValidationErrors_ShouldConsolidateToNINAS()
        {
            // Arrange
            var errors = new Dictionary<string, List<string>>
            {
                { "NationalInsuranceNumber", new List<string> { "Please select one option" } },
                { "NationalAsylumSeekerServiceNumber", new List<string> { "Please select one option" } },
                { "OtherField", new List<string> { "Other Error" } }
            };
            var errorsJson = JsonConvert.SerializeObject(errors);

            // Act
            var result = await _sut.ExecuteAsync(validationErrorsJson: errorsJson);

            // Assert
            result.ValidationErrors.Should().NotContainKey("NationalInsuranceNumber");
            result.ValidationErrors.Should().NotContainKey("NationalAsylumSeekerServiceNumber");
            result.ValidationErrors.Should().ContainKey("NINAS")
                .WhoseValue.Should().ContainSingle()
                .Which.Should().Be("Please select one option");
            result.ValidationErrors.Should().ContainKey("OtherField");
        }

        [Test]
        public async Task ExecuteAsync_WithDifferentNINandNASSErrors_ShouldNotConsolidate()
        {
            // Arrange
            var errors = new Dictionary<string, List<string>>
            {
                { "NationalInsuranceNumber", new List<string> { "Different error" } },
                { "NationalAsylumSeekerServiceNumber", new List<string> { "Another error" } }
            };
            var errorsJson = JsonConvert.SerializeObject(errors);

            // Act
            var result = await _sut.ExecuteAsync(validationErrorsJson: errorsJson);

            // Assert
            result.ValidationErrors.Should().ContainKey("NationalInsuranceNumber");
            result.ValidationErrors.Should().ContainKey("NationalAsylumSeekerServiceNumber");
            result.ValidationErrors.Should().NotContainKey("NINAS");
        }
    }
}