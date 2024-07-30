using CheckYourEligibility_FrontEnd.Models;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;

namespace CheckYourEligibility_Parent.Tests.Attributes
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
        public void Given_NullRequiredFields_When_Validated_Should_ReturnRequiredFieldErrors(string? firstName, string? lastName, int? day, int? month, int? year)
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
            _validationResults[0].ErrorMessage.Should().BeEquivalentTo("First Name is required");
            _validationResults[1].ErrorMessage.Should().BeEquivalentTo("Last Name is required");
            _validationResults[2].ErrorMessage.Should().BeEquivalentTo("Day is required");
            _validationResults[3].ErrorMessage.Should().BeEquivalentTo("Month is required");
            _validationResults[4].ErrorMessage.Should().BeEquivalentTo("Year is required");
            _validationResults.Count().Should().Be(5);
        }
    }
}
