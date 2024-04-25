using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_FrontEnd.Tests.Attributes.Derived
{
    public class TestableNameAttribute : NameAttribute
    {
        public ValidationResult NameIsValid(object value, ValidationContext validationContext)
        {
            return IsValid(value, validationContext);
        }
    }
}
