using CheckYourEligibility_FrontEnd.Models;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_Parent.Tests.Attributes
{
    public class RangeAttributeTests
    {
        private Parent _parent { get; set; }
        private ValidationContext _validationContext { get; set; }
        private List<ValidationResult> _validationResults { get; set; }

        [SetUp]
        public void Setup()
        {
            _parent = new Parent()
            {
                NationalInsuranceNumber = "AB123456C",
                NationalAsylumSeekerServiceNumber = "232300001",
                FirstName = "Homer",
                LastName = "Simpson"
            };
            _validationContext = new ValidationContext(_parent);
            _validationResults = new List<ValidationResult>();
        }

        [TestCase("01", "01", "2000", 0)]
        [TestCase("00", "12", "2023", 1)]
        [TestCase("32", "12", "1990", 1)]
        [TestCase("01", "50", "1990", 1)]
        [TestCase("01", "08", "9999", 1)]
        public void Given_InvalidRangeValues_When_Validated_Should_ReturnExpectedNumberOfErrors(string? day, string? month, string? year, int numberOfErrors)
        {
            // Arrange
            _parent.Day = day;
            _parent.Month = month;
            _parent.Year = year;

            // Act
            Validator.TryValidateObject(_parent, _validationContext, _validationResults, true);

            // Assert
            numberOfErrors.Should().Be(_validationResults.Count);
        }
    }
}
