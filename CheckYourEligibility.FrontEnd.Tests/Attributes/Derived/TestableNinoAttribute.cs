using CheckYourEligibility.FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility.FrontEnd.Tests.Attributes.Derived
{
    public class TestableNinoAttribute : NinoAttribute
    {
        public ValidationResult NinoIsValid(object value, ValidationContext validationContext)
        {
            return IsValid(value, validationContext);
        }
    }
}
