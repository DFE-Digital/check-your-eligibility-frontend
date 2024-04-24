using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CheckYourEligibility_FrontEnd.Attributes
{
    public class NameAttribute : ValidationAttribute
    {
        private static readonly string UnicodeOnlyPattern = @"^\p{L}+$";

        private static readonly Regex regex = new Regex(UnicodeOnlyPattern);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (ParentDetailsViewModel)validationContext.ObjectInstance;

            if (model.FirstName == value)
            {
                if (value == null || value == "")
                    return new ValidationResult("First Name is required");

                if (!regex.IsMatch(value.ToString()))
                    return new ValidationResult("First Name field contains an invalid character");
            }

            if (model.LastName == value)
            {
                if (value == null || value == "")
                    return new ValidationResult("Last Name is required");

                if (!regex.IsMatch(value.ToString()))
                    return new ValidationResult("Last Name field contains an invalid character");
            }       

            return ValidationResult.Success;
        }
    }
}
