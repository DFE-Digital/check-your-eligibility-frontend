using CheckYourEligibility_FrontEnd.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CheckYourEligibility_FrontEnd.Attributes
{
    public class NinoAttribute : ValidationAttribute
    {
        private static readonly string FirstLetterPattern = "[ABCEGHJKLMNOPRSTWXYZ]";
        private static readonly string SecondLetterPattern = "[ABCEGHJKLMNPRSTWXYZ]";
        private static readonly string DisallowedPrefixesPattern = "^(?!BG|GB|KN|NK|NT|TN|ZZ)"; 
        private static readonly string NumericPattern = "[0-9]{6}";
        private static readonly string LastLetterPattern = "[ABCD]";

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

            string nino = value.ToString().ToUpper();

            if (!regex.IsMatch(nino))
            {
                return new ValidationResult("Invalid National Insurance Number format");
            }

            return ValidationResult.Success;
        }
    }
}