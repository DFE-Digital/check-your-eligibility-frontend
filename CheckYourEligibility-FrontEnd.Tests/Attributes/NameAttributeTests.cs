using CheckYourEligibility_FrontEnd.Tests.Attributes.Derived;
using CheckYourEligibility_FrontEnd.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_FrontEnd.Tests.Attributes
{
    public class NameAttributeTests
    {
        const string FirstNameMissingErrorMessage = "First Name is required";
        const string FirstNameFormatErrorMessage = "First Name field contains an invalid character";

        const string LastNameMissingErrorMessage = "Last Name is required";
        const string LastNameFormatErrorMessage = "Last Name field contains an invalid character";

        private TestableNameAttribute _nameAttribute { get; set; }
        private ValidationContext _validationContext { get; set; }
        private ParentDetailsViewModel _parentDetailsViewModel { get; set; }


        [SetUp]
        public void Setup()
        {
            _parentDetailsViewModel = new ParentDetailsViewModel();
            _nameAttribute = new TestableNameAttribute();
        }

        [TestCase(null, FirstNameMissingErrorMessage)]
        [TestCase("", FirstNameMissingErrorMessage)]
        [TestCase("Homer1", FirstNameFormatErrorMessage)]
        [TestCase("Ned2", FirstNameFormatErrorMessage)]
        [TestCase("Seymour!", FirstNameFormatErrorMessage)]
        public void CheckInvalidFirstNames(string? name, string? errorMessage)
        {
            _parentDetailsViewModel.FirstName = name;
            _parentDetailsViewModel.LastName = "SomeLastName";
            _validationContext = new ValidationContext(_parentDetailsViewModel);

            var result = _nameAttribute.NameIsValid(name, _validationContext);

            Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
        }

        [TestCase("Homer")]
        [TestCase("Ned")]
        [TestCase("Seymour")]
        public void CheckValidFirstNames(string? name)
        {
            _parentDetailsViewModel.FirstName = name;
            _parentDetailsViewModel.LastName = "SomeLastName";
            _validationContext = new ValidationContext(_parentDetailsViewModel);

            var result = _nameAttribute.NameIsValid(name, _validationContext);

            Assert.AreEqual(result, null);
        }

        [TestCase(null, LastNameMissingErrorMessage)]
        [TestCase("", LastNameMissingErrorMessage)]
        [TestCase("Simpson1", LastNameFormatErrorMessage)]
        [TestCase("Flanders2", LastNameFormatErrorMessage)]
        [TestCase("Skinner!", LastNameFormatErrorMessage)]
        public void CheckInvalidLastNames(string? name, string? errorMessage)
        {
            _parentDetailsViewModel.FirstName = "SomeFirstName";
            _parentDetailsViewModel.LastName = name;
            _validationContext = new ValidationContext(_parentDetailsViewModel);

            var result = _nameAttribute.NameIsValid(name, _validationContext);

            Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
        }

        [TestCase("Simpson")]
        [TestCase("Flanders")]
        [TestCase("Skinner")]
        public void CheckValidLastNames(string? name)
        {
            _parentDetailsViewModel.FirstName = "SomeFirstName";
            _parentDetailsViewModel.LastName = name;
            _validationContext = new ValidationContext(_parentDetailsViewModel);

            var result = _nameAttribute.NameIsValid(name, _validationContext);

            Assert.AreEqual(result, null);
        }
    }
}
