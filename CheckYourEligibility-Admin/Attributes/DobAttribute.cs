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
        var type = model.GetType();
        var memberName = validationContext.MemberName;

        var dayProperty = type.GetProperty(_dayPropertyName);
        var monthProperty = type.GetProperty(_monthPropertyName);
        var yearProperty = type.GetProperty(_yearPropertyName);

        var dayString = dayProperty.GetValue(model) as string;
        var monthString = monthProperty.GetValue(model) as string;
        var yearString = yearProperty.GetValue(model) as string;

        if (dayProperty == null || monthProperty == null || yearProperty == null)
        {
            return new ValidationResult("Enter a date of birth");
        }

        bool allFieldsEmpty = string.IsNullOrWhiteSpace(dayString) && string.IsNullOrWhiteSpace(monthString) && string.IsNullOrWhiteSpace(yearString);
        bool anyFieldFilled = !string.IsNullOrWhiteSpace(dayString) || !string.IsNullOrWhiteSpace(monthString) || !string.IsNullOrWhiteSpace(yearString);


        if (_isRequired)
        {
            if (allFieldsEmpty)
            {
                return new ValidationResult("Enter a date of birth", new[] { validationContext.MemberName });
            }
        }
        else
        {
            if(allFieldsEmpty)
            {
                return ValidationResult.Success;
            }
        }

        if (anyFieldFilled && !(!string.IsNullOrWhiteSpace(dayString) && !string.IsNullOrWhiteSpace(monthString) && !string.IsNullOrWhiteSpace(yearString)))
        {
            return new ValidationResult("Enter a complete date of birth", new[] { validationContext.MemberName });
        }

        if (!int.TryParse(dayString, out int day))
        {
            return new ValidationResult("Enter a day using numbers only", new[] { validationContext.MemberName });
        }

        if (!int.TryParse(monthString, out int month))
        {
            return new ValidationResult("Enter a month using numbers only", new[] { validationContext.MemberName });
        }

        if (!int.TryParse(yearString, out int year))
        {
            return new ValidationResult("Enter a year using numbers only", new[] { validationContext.MemberName });
        }

        if (day < 1 || day > 31)
        {
            return new ValidationResult("Enter a valid day", new[] { validationContext.MemberName });
        }

        if (month < 1 || month > 12)
        {
            return new ValidationResult("Enter a valid month", new[] { validationContext.MemberName });
        }


        var dob = new DateTime(year, month, day);

        if (dob > DateTime.Now)
        {
            return new ValidationResult("Enter a date in the past", new[] { validationContext.MemberName });
        }

        if (year < 1900 || year > DateTime.Now.Year)
        {
            return new ValidationResult("Enter a valid year", new[] { validationContext.MemberName });
        }

        if (day > DateTime.DaysInMonth(year, month))
        {
            return new ValidationResult("Enter a valid day", new[] { validationContext.MemberName });
        }

        if (_applyAgeRange)
        {
            int age = CalculateAge(dob, DateTime.Now);

            if (age < 4 || age > 19)
            {
                return new ValidationResult("Enter an age between 4 and 19", new[] { validationContext.MemberName });
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
