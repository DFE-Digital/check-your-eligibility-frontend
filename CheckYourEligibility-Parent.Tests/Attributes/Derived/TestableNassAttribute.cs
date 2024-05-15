using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_Parent.Tests.Attributes.Derived
{
    public class TestableNassAttribute : NassAttribute
    {
        public ValidationResult NassIsValid(object value, ValidationContext validationContext)
        {
            return IsValid(value, validationContext);
        }
    }
}
