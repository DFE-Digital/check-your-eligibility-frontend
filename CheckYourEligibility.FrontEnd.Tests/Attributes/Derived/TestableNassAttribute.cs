using CheckYourEligibility.FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility.FrontEnd.Tests.Attributes.Derived
{
    public class TestableNassAttribute : NassAttribute
    {
        public ValidationResult NassIsValid(object value, ValidationContext validationContext)
        {
            return IsValid(value, validationContext);
        }
    }
}
