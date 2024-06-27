﻿using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_Parent.Tests.Attributes.Derived;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_Parent.Tests.Attributes
{
    public class NameAttributeTests
    {
        const string FirstNameFormatErrorMessage = "First Name field contains an invalid character";
        const string LastNameFormatErrorMessage = "Last Name field contains an invalid character";

        private TestableNameAttribute _nameAttribute { get; set; }
        private ValidationContext _validationContext { get; set; }
        private Parent _parent { get; set; }

        [SetUp]
        public void Setup()
        {
            _parent = new Parent();
            _nameAttribute = new TestableNameAttribute();
        }

        [TestCase("Homer1", FirstNameFormatErrorMessage)]
        [TestCase("Ned2", FirstNameFormatErrorMessage)]
        [TestCase("Seymour!", FirstNameFormatErrorMessage)]
        public void Given_FirstName_When_ContainsInvalidCharacters_Should_ReturnErrorMessage(string? name, string? errorMessage)
        {
            // Arrange
            _parent.FirstName = name;
            _parent.LastName = "SomeLastName";
            _validationContext = new ValidationContext(_parent);

            // Act
            var result = _nameAttribute.NameIsValid(name, _validationContext);

            // Assert
            Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
        }

        [TestCase("Homer")]
        [TestCase("Ned")]
        [TestCase("Seymour")]
        public void Given_FirstName_When_Valid_Should_ReturnNull(string? name)
        {
            // Arrange
            _parent.FirstName = name;
            _parent.LastName = "SomeLastName";
            _validationContext = new ValidationContext(_parent);

            // Act
            var result = _nameAttribute.NameIsValid(name, _validationContext);
            result.Should().BeEquivalentTo<ValidationResult>(ValidationResult.Success);

            // Assert
            Assert.AreEqual(result, null);
        }

        [TestCase("Simpson1", LastNameFormatErrorMessage)]
        [TestCase("Flanders2", LastNameFormatErrorMessage)]
        [TestCase("Skinner!", LastNameFormatErrorMessage)]
        public void Given_LastName_When_ContainsInvalidCharacters_Should_ReturnErrorMessage(string? name, string? errorMessage)
        {
            // Arrange
            _parent.FirstName = "SomeFirstName";
            _parent.LastName = name;
            _validationContext = new ValidationContext(_parent);

            // Act
            var result = _nameAttribute.NameIsValid(name, _validationContext);

            // Assert
            Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
        }

        [TestCase("Simpson")]
        [TestCase("Flanders")]
        [TestCase("Skinner")]
        public void Given_LastName_When_Valid_Should_ReturnNull(string? name)
        {
            // Arrange
            _parent.FirstName = "SomeFirstName";
            _parent.LastName = name;
            _validationContext = new ValidationContext(_parent);

            // Act
            var result = _nameAttribute.NameIsValid(name, _validationContext);

            // Assert
            result.Should().BeEquivalentTo<ValidationResult>(ValidationResult.Success);
            Assert.AreEqual(result, null);
        }
    }
}
