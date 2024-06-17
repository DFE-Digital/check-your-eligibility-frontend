using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_Parent.Tests.Attributes.Derived;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_Parent.Tests.Attributes
{
    public class NassAttributeTests
    {
        const string NASSMissingErrorMessage = "Nass is required";
        const string NASSFormatErrorMessage = "Nass field contains an invalid character";

        private TestableNassAttribute _nassAttribute { get; set; }
        private ValidationContext _validationContext { get; set; }
        private Parent _parent { get; set; }

        [SetUp]
        public void Setup()
        {
            _parent = new Parent()
            {
                IsNassSelected = true,
            };
            _nassAttribute = new TestableNassAttribute();
            _validationContext = new ValidationContext(_parent);
        }

        [TestCase(null, NASSMissingErrorMessage)]
        [TestCase("a12345678", NASSFormatErrorMessage)]
        [TestCase("991312345", NASSFormatErrorMessage)]
        [TestCase("991312345", NASSFormatErrorMessage)]
        public void Given_Nass_When_ContainsInvalidCharactersOrIsNull_Should_ReturnErrorMessage(string? nass, string? errorMessage)
        {
            // Act
            var result = _nassAttribute.NassIsValid(nass, _validationContext);

            // Assert
            Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
        }

        [TestCase("010112345")]
        [TestCase("991200001")]
        [TestCase("9912000001")]
        public void Given_Nass_When_Valid_Should_ReturnNull(string? nass)
        {
            // Act
            var result = _nassAttribute.NassIsValid(nass, _validationContext);

            // Assert
            Assert.AreEqual(result, null);
        }
    }
}
