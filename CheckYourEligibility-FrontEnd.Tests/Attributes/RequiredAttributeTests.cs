using CheckYourEligibility_FrontEnd.ViewModels;
using System.ComponentModel.DataAnnotations;
namespace CheckYourEligibility_FrontEnd.Tests.Attributes
{
    public class RequiredAttributeTests
    {
        private ParentDetailsViewModel _parentDetailsViewModel { get; set; }
        private ValidationContext _validationContext { get; set; }
        private List<ValidationResult> _validationResults { get; set; }

        [SetUp]
        public void Setup()
        {
            _parentDetailsViewModel = new ParentDetailsViewModel();
            _validationContext = new ValidationContext(_parentDetailsViewModel);
            _validationResults = new List<ValidationResult>();
        }

        [TestCase(null, null, null, null, null)]
        public void RequiredAttributesFunctionCorrectly(string? firstName, string? lastName, int? day, int? month, int? year)
        {
            _parentDetailsViewModel.FirstName = firstName;
            _parentDetailsViewModel.LastName = lastName;
            _parentDetailsViewModel.Day = day;
            _parentDetailsViewModel.Month = month;
            _parentDetailsViewModel.Year = year;

            Validator.TryValidateObject(_parentDetailsViewModel, _validationContext, _validationResults);

            Assert.True(_validationResults[0].ErrorMessage == "First Name is required");
            Assert.True(_validationResults[1].ErrorMessage == "Last Name is required");
            Assert.True(_validationResults[2].ErrorMessage == "Day is required");
            Assert.True(_validationResults[3].ErrorMessage == "Month is required");
            Assert.True(_validationResults[4].ErrorMessage == "Year is required");
            Assert.AreEqual(5, _validationResults.Count);
        }
    }
}
