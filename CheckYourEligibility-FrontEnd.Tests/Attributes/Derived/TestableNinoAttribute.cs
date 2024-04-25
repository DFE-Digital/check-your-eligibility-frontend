using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_FrontEnd.Tests.Attributes.Derived
{
    public class TestableNinoAttribute : NinoAttribute
    {
        public ValidationResult NinoIsValid(object value, ValidationContext validationContext)
        {
            return IsValid(value, validationContext);
        }
    }
}
