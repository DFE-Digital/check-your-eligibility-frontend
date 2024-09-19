using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;
using System.Reflection;

public class DobAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;

        var dayProp = model.GetType().GetProperty("Day");
        var monthProp = model.GetType().GetProperty("Month");
        var yearProp = model.GetType().GetProperty("Year");

        var day = dayProp?.GetValue(model) as int?;
        var month = monthProp?.GetValue(model) as int?;
        var year = yearProp?.GetValue(model) as int?;

        if (!day.HasValue && !month.HasValue && !year.HasValue)
        {
            return new ValidationResult("Enter a date of birth", new[] { "DateOfBirth" });
        }
        else if (!day.HasValue || !month.HasValue || !year.HasValue)
        {
            return new ValidationResult("Enter a complete date of birth", new[] { "DateOfBirth" });
        }
        if(day.Value < 1 || day.Value > 31)
        {
            return new ValidationResult("Enter a valid day", new[] { "DateOfBirth" });
        }

        if(month.Value < 1 || month.Value > 13)
        {
            return new ValidationResult("Enter a valid month", new[] { "DateOfBirth" });
        }

        if(day.Value > DateTime.DaysInMonth(year.Value, month.Value))
        {
            return new ValidationResult("Enter a valid day", new[] { "DateOfBirth" });
        }

        string dayString = day.Value.ToString("00");
        string monthString = month.Value.ToString("00");
        string yearString = year.Value.ToString();

        if (DateTime.TryParseExact($"{yearString}-{monthString}-{dayString}", "yyyy-MM-dd",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out DateTime dob))
        {
            if (dob > DateTime.Now)
            {
                return new ValidationResult("Date of birth cannot be in the future", new[] { "DateOfBirth" });
            }

            // Additional validation for Child model
            if (model.GetType().Name == "Child")
            {
                int age = CalculateAge(dob, DateTime.Now);

                if (age < 4 || age > 17)
                {
                    return new ValidationResult("Enter an age between 4 and 17", new[] { "DateOfBirth" });
                }
            }

            return ValidationResult.Success;
        }
        else
        {
            return new ValidationResult("Enter a valid date of birth", new[] { "DateOfBirth" });
        }
    }
    private int CalculateAge(DateTime birthDate, DateTime now)
    {
        int age = now.Year - birthDate.Year;
        if (now < birthDate.AddYears(age))
        {
            age--;
        }
        return age;
    }
}

