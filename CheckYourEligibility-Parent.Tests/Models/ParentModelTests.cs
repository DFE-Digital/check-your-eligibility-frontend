using CheckYourEligibility_FrontEnd.Models;
using System.ComponentModel.DataAnnotations;

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

        [TestCase(null, null, true ,null, null, null, null, null, 6)]
        [TestCase(null, null, false ,null, null, null, null, null, 6)]
        //[TestCase("ZZ123456C", null, false ,"Homer", "Simpson", 23, 01, 1990, 1)]
        //[TestCase("", "230400001", true ,"Homer", "Simpson", 23, 01, 1990, 0)]
        //[TestCase("", "230400001", false ,"Homer", "Simpson", 23, 01, 1990, 1)]
        //[TestCase("AB123456C", null, false ,"Homer", "Simpson", 23, 01, 9999, 1)]
        [TestCase("AB123456C", null, false ,"Homer", "Simpson", 32, 01, 1990, 2)]
        [TestCase("AB123456C", null, false ,"Homer", "Simpson", 31, 13, 1990, 2)]
        [TestCase("GB123456A", null, false ,"Homer", "Simpson", 32, 13, 1990, 4)]
        public void ParentModelObject_ValidatesCorrectly(string? nino, string? nass, bool isNassSelected, string? firstName, string? lastName, int? day, int? month, int? year, int numberOfErrors)
        {
            _parent.NationalInsuranceNumber = nino;
            _parent.NationalAsylumSeekerServiceNumber = nass;
            _parent.IsNassSelected = isNassSelected;
            _parent.FirstName = firstName;
            _parent.LastName = lastName;
            _parent.Day = day;
            _parent.Month = month;
            _parent.Year = year;

            Validator.TryValidateObject(_parent, _validationContext, _validationResults, true);

            Assert.AreEqual(numberOfErrors, _validationResults.Count);
        }
    }
}
