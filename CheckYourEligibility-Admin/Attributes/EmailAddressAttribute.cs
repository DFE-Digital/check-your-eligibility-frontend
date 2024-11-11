using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Net;
using System.Text.RegularExpressions;

public class EmailAddressAttribute : ValidationAttribute
{
    

    private const string LocalPartPattern = @"^[a-zA-Z0-9]+([._-][a-zA-Z0-9]+)*$";
    private const string DomainPartPattern = @"^[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,63}$";

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (!(value is string valueAsString))
        {
            return new ValidationResult("Enter an email address in the correct format, like name@example.com");
        }

        if (valueAsString.Contains(' ') || valueAsString.Length > 320)
        {
            return new ValidationResult("Enter an email address in the correct format, like name@example.com");
        }

        int index = valueAsString.IndexOf('@');
        if (index <= 0 || index == valueAsString.Length - 1 || index != valueAsString.LastIndexOf('@'))
        {
            return new ValidationResult("Enter an email address in the correct format, like name@example.com");
        }

        string localPart = valueAsString.Substring(0, index);
        string domainPart = valueAsString.Substring(index + 1);

        if (!IsValidLocalPart(localPart) || !IsValidDomainPart(domainPart))
        {
            return new ValidationResult("Enter an email address in the correct format, like name@example.com");
        }

        return ValidationResult.Success;
    }

    private bool IsValidLocalPart(string localPart)
    {
        return Regex.IsMatch(localPart, LocalPartPattern);
    }

    private bool IsValidDomainPart(string domainPart)
    {
        return Regex.IsMatch(domainPart, DomainPartPattern);
    }
}
