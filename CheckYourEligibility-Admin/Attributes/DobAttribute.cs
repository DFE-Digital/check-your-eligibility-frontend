using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.WebSockets;
using System.Text.RegularExpressions;

namespace CheckYourEligibility_FrontEnd.Attributes
{
    public class DobAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = validationContext.ObjectInstance;

            if (value == null || value == "")
            {
                return ValidationResult.Success;
            }
            else
            {
                DateOnly dob;
                var day = model.GetType().GetProperty("Day").GetValue(model);
                var month = model.GetType().GetProperty("Month").GetValue(model);
                var year = model.GetType().GetProperty("Year").GetValue(model);
                DateOnly dobAsDate = new DateOnly((int)year, (int)month, (int)day);
                var dobString = $"{year}-{month}-{day}";

                if (dobAsDate > DateOnly.FromDateTime(DateTime.Now))
                {
                    return new ValidationResult("Date of Birth cannot be a future date");
                }

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
