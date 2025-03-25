using System.ComponentModel.DataAnnotations;
using CheckYourEligibility.FrontEnd.Attributes;

namespace CheckYourEligibility.FrontEnd.Tests.Attributes.Derived;

public class TestableNinoAttribute : NinoAttribute
{
    public ValidationResult NinoIsValid(object value, ValidationContext validationContext)
    {
        return IsValid(value, validationContext);
    }
}