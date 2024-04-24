using CheckYourEligibility_FrontEnd.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_FrontEnd.Tests.Attributes
{
    public class RangeAttributeTests
    {
        private ParentDetailsViewModel _parentDetailsViewModel { get; set; }
        private ValidationContext _validationContext { get; set; }
        private List<ValidationResult> _validationResults { get; set; }

        [SetUp]
        public void Setup()
        {
            _parentDetailsViewModel = new ParentDetailsViewModel()
            {
                NationalInsuranceNumber = "AB123456C",
                NationalAsylumSeekerServiceNumber = "232300001",
                FirstName = "Homer",
                LastName = "Simpson"
            };
            _validationContext = new ValidationContext(_parentDetailsViewModel);
            _validationResults = new List<ValidationResult>();
        }


        [TestCase(01, 01, 2000, 0)]
        [TestCase(00, 12, 2023, 1)]
        [TestCase(32, 12, 1990, 1)]
        [TestCase(50, 50, 1990, 2)]
        [TestCase(32, 13, 9999, 3)]
        public void RangeAttributesFunctionCorrectly(int? day, int? month, int? year, int numberOfErrors)
        {
            _parentDetailsViewModel.Day = day;
            _parentDetailsViewModel.Month = month;
            _parentDetailsViewModel.Year = year;

            Validator.TryValidateObject(_parentDetailsViewModel, _validationContext, _validationResults, true);

            Assert.AreEqual(numberOfErrors, _validationResults.Count);
        }
    }
}
