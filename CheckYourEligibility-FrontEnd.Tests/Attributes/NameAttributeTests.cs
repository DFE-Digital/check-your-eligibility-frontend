using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Tests.Attributes.Derived;
using CheckYourEligibility_FrontEnd.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_FrontEnd.Tests.Attributes
{
    public class NameAttributeTests
    {
        const string FirstNameFormatErrorMessage = "First Name field contains an invalid character";
        const string LastNameFormatErrorMessage = "Last Name field contains an invalid character";

        private TestableNameAttribute _nameAttribute { get; set; }
        private ValidationContext _validationContext { get; set; }
        private Parent _parent { get; set; }
        private Child _child { get; set; }


        [SetUp]
        public void Setup()
        {
            _parent = new Parent();
            _child = new Child();
            _nameAttribute = new TestableNameAttribute();
        }

        [TestCase("Homer1", FirstNameFormatErrorMessage)]
        [TestCase("Ned2", FirstNameFormatErrorMessage)]
        [TestCase("Seymour!", FirstNameFormatErrorMessage)]
        public void CheckInvalidFirstNames(string? name, string? errorMessage)
        {
            _parent.FirstName = name;
            _parent.LastName = "SomeLastName";
            _validationContext = new ValidationContext(_parent);

            var result = _nameAttribute.NameIsValid(name, _validationContext);

            Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
        }

        [TestCase("Homer")]
        [TestCase("Ned")]
        [TestCase("Seymour")]
        public void CheckValidFirstNames(string? name)
        {
            _parent.FirstName = name;
            _parent.LastName = "SomeLastName";
            _validationContext = new ValidationContext(_parent);

            var result = _nameAttribute.NameIsValid(name, _validationContext);

            Assert.AreEqual(result, null);
        }

        [TestCase("Simpson1", LastNameFormatErrorMessage)]
        [TestCase("Flanders2", LastNameFormatErrorMessage)]
        [TestCase("Skinner!", LastNameFormatErrorMessage)]
        public void CheckInvalidLastNames(string? name, string? errorMessage)
        {
            _parent.FirstName = "SomeFirstName";
            _parent.LastName = name;
            _validationContext = new ValidationContext(_parent);

            var result = _nameAttribute.NameIsValid(name, _validationContext);

            Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
        }

        [TestCase("Simpson")]
        [TestCase("Flanders")]
        [TestCase("Skinner")]
        public void CheckValidLastNames(string? name)
        {
            _parent.FirstName = "SomeFirstName";
            _parent.LastName = name;
            _validationContext = new ValidationContext(_parent);

            var result = _nameAttribute.NameIsValid(name, _validationContext);

            Assert.AreEqual(result, null);
        }
    }
}
