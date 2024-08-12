using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CheckYourEligibility_FrontEnd.Attributes
{
    public class LastNameAttribute : ValidationAttribute
    {
        private static readonly string UnicodeOnlyPattern = @"^[\p{L}\-']+$";

        private static readonly Regex regex = new Regex(UnicodeOnlyPattern);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            if (string.IsNullOrEmpty(value?.ToString()))
            {
                return ValidationResult.Success;
            }

            if (!regex.IsMatch(value.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }

        
            return ValidationResult.Success;
        }
    }
}
