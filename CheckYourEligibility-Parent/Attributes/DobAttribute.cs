using System.ComponentModel.DataAnnotations;

using CheckYourEligibility_FrontEnd.Models;
using Microsoft.AspNetCore.Authentication;
using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Text;
using System.Globalization;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class DobAttribute : ValidationAttribute
{
    private readonly bool _isRequired;
    private readonly bool _applyAgeRange;
    private readonly string _dayPropertyName;
    private readonly string _monthPropertyName;
    private readonly string _yearPropertyName;




    public DobAttribute(string dayPropertyName, string monthPropertyName, string yearPropertyName, bool isRequired = true, bool applyAgeRange = false, string? errorMessage = null) : base(errorMessage)
    {
        _isRequired = isRequired;
        _applyAgeRange = applyAgeRange;
        _dayPropertyName = dayPropertyName;
        _monthPropertyName = monthPropertyName;
        _yearPropertyName = yearPropertyName;

    }
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;
        //var type = model.GetType();
        //var memberName = validationContext.MemberName;

        var dayString = GetPropertyValue(model, _dayPropertyName);
        var monthString = GetPropertyValue(model, _monthPropertyName);
        var yearString = GetPropertyValue(model, _yearPropertyName);

        bool allFieldsEmpty = string.IsNullOrEmpty(dayString) &&
                      string.IsNullOrEmpty(monthString) &&
                      string.IsNullOrEmpty(yearString);

        if (!_isRequired && allFieldsEmpty)
        {
            return ValidationResult.Success;
        }

        var missingFields = new List<string> { "DateOfBirth" };
        if (string.IsNullOrWhiteSpace(dayString)) missingFields.Add("Day");
        if (string.IsNullOrWhiteSpace(monthString)) missingFields.Add("Month");
        if (string.IsNullOrWhiteSpace(yearString)) missingFields.Add("Year");

        if (missingFields.Count == 4)
        {
            return new ValidationResult("Enter a date of birth", new[] { "DateOfBirth", "Day", "Month", "Year" });
        }

        if (missingFields.Count > 1)
        {
            return new ValidationResult("Enter a complete date of birth", missingFields.ToArray());
        }

        var invalidFields = new List<string> { "DateOfBirth" };

        bool isDayInvalid = !int.TryParse(dayString, out int dayInt);
        if (dayInt < 1 || dayInt > 31) isDayInvalid = true;
        if (isDayInvalid) invalidFields.Add("Day");

        bool isMonthInvalid = !int.TryParse(monthString, out int monthInt);
        if (monthInt < 1 || monthInt > 12) isMonthInvalid = true;
        if (isMonthInvalid) invalidFields.Add("Month");

        bool isYearInvalid = !int.TryParse(yearString, out int yearInt);
        if (yearInt < 1900) isYearInvalid = true;
        if (isYearInvalid) invalidFields.Add("Year");

        if (!int.TryParse(dayString, out _) && !int.TryParse(monthString, out _) && !int.TryParse(yearString, out _))
        {
            return new ValidationResult("Enter a date of birth using numbers only", new[] { "DateOfBirth", "Day", "Month", "Year" });
        }

        if (invalidFields.Count > 2)
        {
            return new ValidationResult("Enter a date using numbers only", invalidFields.ToArray());
        }

        if (isDayInvalid)
        {
            return new ValidationResult("Enter a day using numbers only", new[] { "DateOfBirth", "Day", });
        }


        if (isMonthInvalid)
        {
            return new ValidationResult("Enter a month using numbers only", invalidFields.ToArray());
        }


        if (isYearInvalid)
        {
            return new ValidationResult("Enter a valid year", invalidFields.ToArray());
        }

        try
        {
            var dob = new DateTime(yearInt, monthInt, dayInt);

            if (dob > DateTime.Now)
            {
                return new ValidationResult("Enter a date in the past", new[] { "DateOfBirth", "Day", "Month", "Year" });
            }



            if (dayInt > DateTime.DaysInMonth(yearInt, monthInt))
            {
                return new ValidationResult("Enter a valid day", new[] { "DateOfBirth", "Day" });
            }

            if (_applyAgeRange)
            {
                int age = CalculateAge(dob, DateTime.Now);

                if (age < 4 || age > 19)
                {
                    return new ValidationResult("Enter an age between 4 and 19", new[] { "DateOfBirth", "Day", "Month", "Year" });
                }
            }
        }
        catch
        {
            return new ValidationResult("Enter a valid date of birth");
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