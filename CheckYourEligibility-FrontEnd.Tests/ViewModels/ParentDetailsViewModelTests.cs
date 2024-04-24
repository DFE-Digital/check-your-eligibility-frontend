using CheckYourEligibility_FrontEnd.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckYourEligibility_FrontEnd.Tests.ViewModels
{
    public class ParentDetailsViewModelTests
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

        [TestCase(null, null, true ,null, null, null, null, null, 6)]
        [TestCase(null, null, false ,null, null, null, null, null, 6)]
        [TestCase("ZZ123456C", null, false ,"Homer", "Simpson", 23, 01, 1990, 1)]
        [TestCase("", "230400001", true ,"Homer", "Simpson", 23, 01, 1990, 0)]
        [TestCase("", "230400001", false ,"Homer", "Simpson", 23, 01, 1990, 1)]
        [TestCase("AB123456C", null, false ,"Homer", "Simpson", 23, 01, 9999, 1)]
        [TestCase("AB123456C", null, false ,"Homer", "Simpson", 32, 01, 1990, 1)]
        [TestCase("AB123456C", null, false ,"Homer", "Simpson", 31, 13, 1990, 1)]
        [TestCase("GB123456A", null, false ,"Homer", "Simpson", 32, 13, 1990, 3)]
        public void ParentDetailsViewModelObject_ValidatesCorrectly(string? nino, string? nass, bool isNassSelected, string? firstName, string? lastName, int? day, int? month, int? year, int numberOfErrors)
        {
            _parentDetailsViewModel.NationalInsuranceNumber = nino;
            _parentDetailsViewModel.NationalAsylumSeekerServiceNumber = nass;
            _parentDetailsViewModel.IsNassSelected = isNassSelected;
            _parentDetailsViewModel.FirstName = firstName;
            _parentDetailsViewModel.LastName = lastName;
            _parentDetailsViewModel.Day = day;
            _parentDetailsViewModel.Month = month;
            _parentDetailsViewModel.Year = year;

            Validator.TryValidateObject(_parentDetailsViewModel, _validationContext, _validationResults, true);

            Assert.AreEqual(numberOfErrors, _validationResults.Count);
        }
    }
}
