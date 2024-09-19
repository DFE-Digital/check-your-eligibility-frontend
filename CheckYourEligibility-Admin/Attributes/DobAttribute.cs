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
    private readonly string _dayPropertyName;
    private readonly string _monthPropertyName;
    private readonly string _yearPropertyName;




    public DobAttribute(string dayPropertyName, string monthPropertyName, string yearPropertyName, bool isRequired = true)
    {
        _isRequired = isRequired;
        _dayPropertyName = dayPropertyName;
        _monthPropertyName = monthPropertyName;
        _yearPropertyName = yearPropertyName;

    }
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;
        var type = model.GetType();

        var dayProperty = type.GetProperty(_dayPropertyName);
        var monthProperty = type.GetProperty(_monthPropertyName);
        var yearProperty = type.GetProperty(_yearPropertyName);

        if (dayProperty == null && monthProperty == null && yearProperty == null)
        {
            return new ValidationResult("Enter a date of birth");
        }

        var dayString = dayProperty?.GetValue(model) as string;
        var monthString = monthProperty?.GetValue(model) as string;
        var yearString = yearProperty?.GetValue(model) as string;

        bool anyFieldFilled = !string.IsNullOrWhiteSpace(dayString) || !string.IsNullOrWhiteSpace(monthString) || !string.IsNullOrWhiteSpace(yearString);
        bool allFieldsEmpty = string.IsNullOrWhiteSpace(dayString) || string.IsNullOrWhiteSpace(monthString) || string.IsNullOrWhiteSpace(yearString);

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

        if (anyFieldFilled && !(!string.IsNullOrEmpty(dayString) && !string.IsNullOrEmpty(monthString) && !string.IsNullOrEmpty(yearString)))
        {
            return new ValidationResult("Enter a complete date of birth", new[] { validationContext.MemberName });
        }

        if (!int.TryParse(dayString, out int dayInt))
        {
            return new ValidationResult("Enter a day using numbers only", new[] { validationContext.MemberName });
        }

        if (dayInt < 1 || dayInt > 31)
        {
            return new ValidationResult("Enter a valid day", new[] { validationContext.MemberName });
        }

        if (!int.TryParse(monthString, out int monthInt))
        {
            return new ValidationResult("Enter a month using numbers only", new[] { validationContext.MemberName });
        }

        if (monthInt < 1 || monthInt > 12)
        {
            return new ValidationResult("Enter a valid month", new[] { validationContext.MemberName });
        }

        if (!int.TryParse(yearString, out int yearInt))
        {
            return new ValidationResult("Enter a year using numbers only", new[] { validationContext.MemberName });
        }

        var dob = new DateTime(yearInt, monthInt, dayInt);

        if (dob > DateTime.Now)
        {
            return new ValidationResult("Enter a date in the past", new[] { validationContext.MemberName });
        }

        if (yearInt < 1900 || yearInt > DateTime.Now.Year)
        {
            return new ValidationResult("Enter a valid year", new[] { validationContext.MemberName });
        }

        if (dayInt > DateTime.DaysInMonth(yearInt, monthInt))
        {
            return new ValidationResult("Enter a valid day", new[] { validationContext.MemberName });
        }

        if (model is Child)
        {
            int age = CalculateAge(dob, DateTime.Now);

            if (age < 4 || age > 17)
            {
                return new ValidationResult("Enter an age between 4 and 17", new[] { validationContext.MemberName });
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
