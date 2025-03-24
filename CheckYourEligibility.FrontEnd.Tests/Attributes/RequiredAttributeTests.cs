using CheckYourEligibility.FrontEnd.Models;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;

namespace CheckYourEligibility.FrontEnd.Tests.Attributes
{
    public class RequiredAttributeTests
    {
        private Parent _parent { get; set; }
        private ValidationContext _validationContext { get; set; }
        private List<ValidationResult> _validationResults { get; set; }

        [SetUp]
        public void Setup()
        {
            _parent = new Parent();
            _validationContext = new ValidationContext(_parent);
            _validationResults = new List<ValidationResult>();
        }

        [TestCase(null, null, null, null, null)]
        public void Given_NullRequiredFields_When_Validated_Should_ReturnRequiredFieldErrors(string? firstName, string? lastName, string? day, string? month, string? year)
        {
            // Arrange
            _parent.FirstName = firstName;
            _parent.LastName = lastName;
            _parent.Day = day;
            _parent.Month = month;
            _parent.Year = year;

            // Act
            Validator.TryValidateObject(_parent, _validationContext, _validationResults);

            // Assert
            _validationResults[0].ErrorMessage.Should().BeEquivalentTo("Enter a first name");
            _validationResults[1].ErrorMessage.Should().BeEquivalentTo("Enter a last name");
            _validationResults.Count().Should().Be(2);
        }
    }
}
