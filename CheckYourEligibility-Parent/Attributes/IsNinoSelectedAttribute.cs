using CheckYourEligibility_FrontEnd.Models;
using System.ComponentModel.DataAnnotations;
namespace CheckYourEligibility_FrontEnd.Attributes
{
    internal class IsNinoSelectedAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (Parent)validationContext.ObjectInstance;
            if (model.IsNassSelected == null && model.IsNinoSelected == null)
            {
                return new ValidationResult("Select yes if you have a National Insurance number");
            }
            return ValidationResult.Success;
        }
    }
}