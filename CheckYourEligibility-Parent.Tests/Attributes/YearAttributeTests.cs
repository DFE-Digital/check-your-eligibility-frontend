using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_Parent.Tests.Attributes.Derived;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_Parent.Tests.Attributes
{
    public class YearAttributeTests
    {
        const string YearMissingErrorMessage = "Year is required";
        const string YearFormatErrorMessage = "Invalid Year";
        private TestableYearAttribute _yearAttribute { get; set; }
        private ValidationContext _validationContext { get; set; }
        private Parent _parent { get; set; }

        [SetUp]
        public void Setup()
        {
            _parent = new Parent();
            _yearAttribute = new TestableYearAttribute();
            _validationContext = new ValidationContext(_parent);
        }

        [TestCase(1800, YearFormatErrorMessage)]
        [TestCase(2500, YearFormatErrorMessage)]
        public void Given_InvalidYear_When_Validated_Should_ReturnErrorMessage(int? year, string? errorMessage)
        {
            // Act
            var result = _yearAttribute.YearIsValid(year, _validationContext);

            // Assert
            Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
        }

        [TestCase(2023)]
        [TestCase(1990)]
        [TestCase(1950)]
        public void Given_ValidYear_When_Validated_Should_ReturnNull(int? year)
        {
            // Act
            var result = _yearAttribute.YearIsValid(year, _validationContext);

            // Assert
            Assert.AreEqual(result, null);
        }
    }
}
