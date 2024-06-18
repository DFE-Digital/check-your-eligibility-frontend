using CheckYourEligibility_FrontEnd.Models;
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
            var model = (ParentGuardian)validationContext.ObjectInstance;

            if (model.NationalAsylumSeekerServiceNumber != null)
            {
                model.IsNassSelected = true;
            }


            if (model.IsNassSelected == true)
            {
                return ValidationResult.Success;
            }

            if (value == null)
            {
                return new ValidationResult("National Insurance Number or National Asylum Seeker Service Number is required");
            }

            if (value != null)
            {
                string nino = value.ToString().ToUpper();
            string nino = value.ToString().ToUpper();
            nino = String.Concat(nino
                .Where(ch => Char.IsLetterOrDigit(ch)));

            if (nino.Length > 9)
            {
                return new ValidationResult("National Insurance Number should contain no more than 9 alphanumeric characters");
            }

            if (!regex.IsMatch(nino))
            {
                return new ValidationResult("Invalid National Insurance Number format");
            }
            else
            {
                model.NationalInsuranceNumber = nino;
            }

                if (!regex.IsMatch(nino))
                {
                    return new ValidationResult("Invalid National Insurance Number format");
                }
            }
            return ValidationResult.Success;
        }
    }
}