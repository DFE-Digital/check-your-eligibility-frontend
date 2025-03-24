using CheckYourEligibility.FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility.FrontEnd.Tests.Attributes.Derived
{
    public class TestableYearAttribute : YearAttribute
    {
        public ValidationResult YearIsValid(object value, ValidationContext validationContext)
        {
            return IsValid(value, validationContext);
        }
    }
}
