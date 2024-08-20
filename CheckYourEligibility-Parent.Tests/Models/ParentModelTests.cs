using CheckYourEligibility_FrontEnd.Models;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;

namespace CheckYourEligibility_Parent.Tests.ViewModels
{
    public class ParentModelTests
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

        [TestCase(null, null, true, false, null, null, null, null, null, 6)]
        [TestCase(null, null, false, true, null, null, null, null, null, 6)]
        [TestCase("AB123456C", null, false, null, "Homer", "Simpson", 32, 01, 1990, 2)]
        [TestCase("AB123456C", null, false, null, "Homer", "Simpson", 31, 13, 1990, 2)]
        [TestCase("GB123456A", null, false, null, "Homer", "Simpson", 32, 13, 1990, 4)]
        public void Given_InvalidParentModel_When_Validated_Should_ReturnExpectedNumberOfErrors(string? nino, string? nass, bool? isNinoSelected, bool? isNassSelected , string? firstName, string? lastName, int? day, int? month, int? year, int numberOfErrors)
        {
            // Arrange
            _parent.NationalInsuranceNumber = nino;
            _parent.NationalAsylumSeekerServiceNumber = nass;
            _parent.IsNinoSelected = isNinoSelected;
            _parent.IsNassSelected = isNassSelected;
            _parent.FirstName = firstName;
            _parent.LastName = lastName;
            _parent.Day = day;
            _parent.Month = month;
            _parent.Year = year;

            // Act
            Validator.TryValidateObject(_parent, _validationContext, _validationResults, true);

            //
            _validationResults.Count.Should().Be(numberOfErrors);
        }
    }
}
