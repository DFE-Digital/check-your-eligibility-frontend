using System.ComponentModel.DataAnnotations;
using CheckYourEligibility.FrontEnd.Attributes;

namespace CheckYourEligibility.FrontEnd.Tests.Attributes.Derived;

public class TestableYearAttribute : YearAttribute
{
    public ValidationResult YearIsValid(object value, ValidationContext validationContext)
    {
        return IsValid(value, validationContext);
    }
}