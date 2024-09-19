using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

public class DobAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;

        var dayProperty = model.GetType().GetProperty("Day");
        var monthProperty = model.GetType().GetProperty("Month");
        var yearProperty = model.GetType().GetProperty("Year");

        var dayString = dayProperty?.GetValue(model) as string;
        var monthString = monthProperty?.GetValue(model) as string;
        var yearString = yearProperty?.GetValue(model) as string;

        // Check if all date parts are provided
        if (string.IsNullOrWhiteSpace(dayString) && string.IsNullOrWhiteSpace(monthString) && string.IsNullOrWhiteSpace(yearString))
        {
            return new ValidationResult("Enter a date of birth", new[] { "DateOfBirth" });
        }
        else if (string.IsNullOrWhiteSpace(dayString) || string.IsNullOrWhiteSpace(monthString) || string.IsNullOrWhiteSpace(yearString))
        {
            return new ValidationResult("Enter a complete date of birth", new[] { "DateOfBirth" });
        }

        // Parse the day
        if (!int.TryParse(dayString, out int dayInt))
        {
            return new ValidationResult("Enter a day using numbers only", new[] { "DateOfBirth" });
        }

        if (dayInt < 1 || dayInt > 31)
        {
            return new ValidationResult("Enter a valid day", new[] { "DateOfBirth" });
        }

        // Parse the month
        if (!int.TryParse(monthString, out int monthInt))
        {
            return new ValidationResult("Enter a month using numbers only", new[] { "DateOfBirth" });
        }

        if (monthInt < 1 || monthInt > 12)
        {
            return new ValidationResult("Enter a valid month", new[] { "DateOfBirth" });
        }

        // Parse the year
        if (!int.TryParse(yearString, out int yearInt))
        {
            return new ValidationResult("Enter a year using numbers only", new[] { "DateOfBirth" });
        }

        var dob = new DateTime(yearInt, monthInt, dayInt);

        // Check if the date is in the future
        if (dob > DateTime.Now)
        {
            return new ValidationResult("Enter a date in the past", new[] { "DateOfBirth" });
        }

        if (yearInt < 1900 || yearInt > DateTime.Now.Year)
        {
            return new ValidationResult("Enter a valid year", new[] { "DateOfBirth" });
        }

        // Check if the day is valid for the given month and year
        if (dayInt > DateTime.DaysInMonth(yearInt, monthInt))
        {
            return new ValidationResult("Enter a valid day", new[] { "DateOfBirth" });
        }

        // Construct the DateTime object


        // Additional validation for Child model (if applicable)
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
