using System.ComponentModel.DataAnnotations;

public class CuEmailAddressAttribute : ValidationAttribute
{
    private static bool EnableFullDomainLiterals { get; } =
        AppContext.TryGetSwitch("System.Net.AllowFullDomainLiterals", out bool enable) ? enable : false;

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (!(value is string valueAsString))
        {
            return new ValidationResult("Invalid email address format.");
        }

        if (!EnableFullDomainLiterals && (valueAsString.Contains('\r') || valueAsString.Contains('\n')))
        {
            return new ValidationResult("Please enter a valid email address.");
        }

        int index = valueAsString.IndexOf('@');
        if (index <= 0 || index == valueAsString.Length - 1 || index != valueAsString.LastIndexOf('@'))
        {
            return new ValidationResult("Please enter a valid email address.");
        }

        return ValidationResult.Success;
    }
}
