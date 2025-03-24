using CheckYourEligibility.FrontEnd.Models;
using System.ComponentModel.DataAnnotations;
namespace CheckYourEligibility.FrontEnd.Attributes
{
    internal class IsNinoSelectedAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (Parent)validationContext.ObjectInstance;
            if (model.IsNassSelected == null && model.IsNinoSelected == null && model.NASSRedirect == false)
            {
                return new ValidationResult("Select yes if you have a National Insurance number");
            }
            else if (model.IsNassSelected == null && model.IsNinoSelected == false)
            {
                return ValidationResult.Success;
            }
            else if (model.IsNassSelected == null && model.NASSRedirect == true)
            {
                return new ValidationResult("Select yes if you have a National Asylum Seeker Service number");
            }
            return ValidationResult.Success;
        }
    }
}