using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_Parent.Tests.Attributes.Derived
{
    public class TestableYearAttribute : YearAttribute
    {
        public ValidationResult YearIsValid(object value, ValidationContext validationContext)
        {
            return IsValid(value, validationContext);
        }
    }
}
