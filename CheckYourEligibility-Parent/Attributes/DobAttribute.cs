using CheckYourEligibility_FrontEnd.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

public class DobAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;

        var dayString = GetPropertyValue(model, "Day");
        var monthString = GetPropertyValue(model, "Month");
        var yearString = GetPropertyValue(model, "Year");

        var missingFields = new List<string> {"DateOfBirth"};
        if (string.IsNullOrWhiteSpace(dayString)) missingFields.Add("Day");
        if (string.IsNullOrWhiteSpace(monthString)) missingFields.Add("Month");
        if (string.IsNullOrWhiteSpace(yearString)) missingFields.Add("Year");

        if (missingFields.Count == 4)
        {
            return new ValidationResult("Enter a date of birth", new[] { "DateOfBirth", "Day", "Year", "Month" });
        }
        if (missingFields.Count > 1)
        {
            return new ValidationResult("Enter a complete date of birth", missingFields.ToArray() );
        }

        if (!int.TryParse(dayString, out int dayInt))
        {
            return new ValidationResult("Enter a day using numbers only", new[] { "DateOfBirth", "Day" });
        }

        if (dayInt < 1 || dayInt > 31)
        {
            return new ValidationResult("Enter a valid day", new[] { "DateOfBirth", "Day" });
        }

        if (!int.TryParse(monthString, out int monthInt))
        {
            return new ValidationResult("Enter a month using numbers only", new[] { "DateOfBirth", "Month" });
        }

        if (monthInt < 1 || monthInt > 12)
        {
            return new ValidationResult("Enter a valid month", new[] { "DateOfBirth", "Month" });
        }

        if (!int.TryParse(yearString, out int yearInt))
        {
            return new ValidationResult("Enter a year using numbers only", new[] { "DateOfBirth", "Year" });
        }

        if (dayInt > DateTime.DaysInMonth(yearInt, monthInt))
        {
            return new ValidationResult("Enter a valid day", new[] { "DateOfBirth", "Day" });
        }

        var dob = new DateTime(yearInt, monthInt, dayInt);

        if (dob > DateTime.Now)
        {
            return new ValidationResult("Enter a date in the past", new[] { "DateOfBirth", "Day", "Month", "Year" });
        }

        if (yearInt < 1900 || yearInt > DateTime.Now.Year)
        {
            return new ValidationResult("Enter a valid year", new[] { "DateOfBirth", "Year" });
        }

        if (model is Child)
        {
            int age = CalculateAge(dob, DateTime.Now);

            if (age < 4 || age > 19)
            {
                return new ValidationResult("Enter an age between 4 and 19", new[] { "DateOfBirth", "Day", "Month", "Year" });
            }
        }

        return ValidationResult.Success;
    }

    private string GetPropertyValue(object model, string propertyName)
    {
        return model.GetType().GetProperty(propertyName)?.GetValue(model) as string;
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
