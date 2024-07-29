using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
                DateTime dob;
                var day = model.GetType().GetProperty("Day").GetValue(model)?.ToString().PadLeft(2, '0');
                var month = model.GetType().GetProperty("Month").GetValue(model)?.ToString().PadLeft(2, '0');
                var year = model.GetType().GetProperty("Year").GetValue(model)?.ToString().PadLeft(2, '0');

                if (DateTime.TryParseExact($"{year}-{month}-{day}","yyyy-MM-dd",
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out dob))
                {
                    if (dob.Year > DateTime.Now.Year)
                    {
                        return new ValidationResult("Date of Birth cannot be a future date");
                    }
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
