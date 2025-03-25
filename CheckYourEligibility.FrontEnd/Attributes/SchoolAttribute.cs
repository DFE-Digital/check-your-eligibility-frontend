using System.ComponentModel.DataAnnotations;
using CheckYourEligibility.FrontEnd.Models;

namespace CheckYourEligibility.FrontEnd.Attributes;

public class SchoolAttribute : ValidationAttribute
{
    private readonly string _childIndexPropertyName;

    public SchoolAttribute(string childIndexPropertyName)
    {
        _childIndexPropertyName = childIndexPropertyName;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var school = validationContext.ObjectInstance as School;
        var childIndex = school.ChildIndex;

        if (school == null) return new ValidationResult("Invalid school instance.");

        if (childIndex == null) return new ValidationResult("ChildIndex value cannot be null or empty.");

        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult($"Select a school for child {childIndex}");
        return ValidationResult.Success;
    }
}