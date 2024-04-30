using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CheckYourEligibility_FrontEnd.Attributes
{
    public class DobAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (Parent)validationContext.ObjectInstance;

            if (value == null || value == "")
            {
                return ValidationResult.Success;
            }
            else
            {
                DateOnly dob;
                var day = value;
                var month = model.Month;
                var year = model.Year;
                var dobString = $"{day}/{month}/{year}";

                if (DateOnly.TryParse(dobString, out dob))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("Invalid date entered");
                }
            }
        }
    }
}
