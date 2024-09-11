using CheckYourEligibility_FrontEnd.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CheckYourEligibility_FrontEnd.Attributes
{
    public class NinoSelectionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            var model = (Parent)validationContext.ObjectInstance;

            if (model.IsNinoSelected == null && model.IsNassSelected == false)
            {
                return new ValidationResult("Select whether you have a National Insurance number or not");
            }


            return ValidationResult.Success;
        }
    }
}