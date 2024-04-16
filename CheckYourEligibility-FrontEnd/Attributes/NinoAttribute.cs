using CheckYourEligibility_FrontEnd.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CheckYourEligibility_FrontEnd.Attributes
{
    public class NinoAttribute : ValidationAttribute
    {
        private static readonly string FirstLetterPattern = "[A-CEGHJ-NPR-TW-Z]";
        private static readonly string SecondLetterPattern = "[A-NP-Z]";
        private static readonly string DisallowedPrefixesPattern = "^(?!BG|GB|KN|NK|NT|TV|ZZ)";
        private static readonly string NumericPattern = "[0-9]{6}";
        private static readonly string LastLetterPattern = "[A-D]";

        private static readonly string Pattern = DisallowedPrefixesPattern + FirstLetterPattern + SecondLetterPattern + NumericPattern + LastLetterPattern;

        private static readonly Regex regex = new Regex(Pattern);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (ParentDetailsViewModel)validationContext.ObjectInstance;

            if (model.IsNassSelected == true)
            {
                return ValidationResult.Success;
            }

            if (value == null)
            {
                return new ValidationResult("National Insurance Number is required");
            }

            if (!regex.IsMatch(value.ToString()))
            {
                return new ValidationResult("Invalid National Insurance Number format");
            }

            return ValidationResult.Success;
        }
    }
}
