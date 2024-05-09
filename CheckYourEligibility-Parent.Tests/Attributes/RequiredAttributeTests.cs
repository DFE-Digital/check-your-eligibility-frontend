using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.ViewModels;
using System.ComponentModel.DataAnnotations;
namespace CheckYourEligibility_Parent.Tests.Attributes
{
    public class RequiredAttributeTests
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

        [TestCase(null, null, null, null, null)]
        public void RequiredAttributesFunctionCorrectly(string? firstName, string? lastName, int? day, int? month, int? year)
        {
            _parent.FirstName = firstName;
            _parent.LastName = lastName;
            _parent.Day = day;
            _parent.Month = month;
            _parent.Year = year;

            Validator.TryValidateObject(_parent, _validationContext, _validationResults);

            Assert.True(_validationResults[0].ErrorMessage == "First Name is required");
            Assert.True(_validationResults[1].ErrorMessage == "Last Name is required");
            Assert.True(_validationResults[2].ErrorMessage == "Day is required");
            Assert.True(_validationResults[3].ErrorMessage == "Month is required");
            Assert.True(_validationResults[4].ErrorMessage == "Year is required");
            Assert.AreEqual(5, _validationResults.Count);
        }
    }
}
