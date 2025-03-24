using System.ComponentModel.DataAnnotations;

using CheckYourEligibility.FrontEnd.Models;
using Microsoft.AspNetCore.Authentication;
using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Text;
using System.Globalization;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class DobAttribute : ValidationAttribute
{
    private readonly string _fieldName;
    private readonly string _objectName;
    private readonly string _childIndexPropertyName;
    private readonly bool _isRequired;
    private readonly bool _applyAgeRange;
    private readonly string _dayPropertyName;
    private readonly string _monthPropertyName;
    private readonly string _yearPropertyName;




    public DobAttribute(string fieldName, string objectName, string? childIndexPropertyName, string dayPropertyName, string monthPropertyName, string yearPropertyName, bool isRequired = true, bool applyAgeRange = false, string? errorMessage = null) : base(errorMessage)
    {

        _fieldName = fieldName;
        _objectName = objectName;
        _childIndexPropertyName = childIndexPropertyName;
        _dayPropertyName = dayPropertyName;
        _monthPropertyName = monthPropertyName;
        _yearPropertyName = yearPropertyName;
        _isRequired = isRequired;
        _applyAgeRange = applyAgeRange;

    }
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;
        int? childIndex = null;

        // get the child index if it exists, this should return null only if the model is ParentGuardian
        if (validationContext.ObjectInstance.GetType() == typeof(Child))
        {
            model = validationContext.ObjectInstance as Child;
            childIndex = GetPropertyIntValue(model, _childIndexPropertyName);
        }

        var dayString = GetPropertyStringValue(model, _dayPropertyName);
        var monthString = GetPropertyStringValue(model, _monthPropertyName);
        var yearString = GetPropertyStringValue(model, _yearPropertyName);

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
            if (childIndex != null)
            {
                return new ValidationResult($"Enter a {_fieldName} for {_objectName} {childIndex}", new[] { "DateOfBirth", "Day", "Month", "Year" });
            }
            else
            {
                return new ValidationResult($"Enter a {_fieldName}", new[] { "DateOfBirth", "Day", "Month", "Year" });
            }
        }

        if (missingFields.Count > 1)
        {
            if (childIndex != null)
            {
                return new ValidationResult($"Enter a {_fieldName} for {_objectName} {childIndex}", new[] { "DateOfBirth", "Day", "Month", "Year" });
            }
            else
            {
                return new ValidationResult($"Enter a {_fieldName}", new[] { "DateOfBirth", "Day", "Month", "Year" });
            }
        }

        // Check numeric input and ranges
        var invalidFields = new List<string> { "DateOfBirth" };

        bool isDayInvalid = !int.TryParse(dayString, out int dayInt) || dayInt < 1 || dayInt > 31;
        if (isDayInvalid) invalidFields.Add("Day");

        bool isMonthInvalid = !int.TryParse(monthString, out int monthInt) || monthInt < 1 || monthInt > 12;
        if (isMonthInvalid) invalidFields.Add("Month");

        bool isYearInvalid = !int.TryParse(yearString, out int yearInt) || yearInt < 1900;
        if (isYearInvalid) invalidFields.Add("Year");

        // Handle non-numeric inputs
        if (!int.TryParse(dayString, out _) || !int.TryParse(monthString, out _) || !int.TryParse(yearString, out _))
        {
            if (childIndex != null)
            {
                return new ValidationResult($"Enter a {_fieldName} using numbers only for {_objectName} {childIndex}", new[] { "DateOfBirth", "Day", "Month", "Year" });
            }
            else
            {
                return new ValidationResult($"Enter a {_fieldName} using numbers only", new[] { "DateOfBirth", "Day", "Month", "Year" });
            }
        }

        // Handle invalid fields
        if (invalidFields.Count > 1)
        {
            if (childIndex != null)
            {
                return new ValidationResult($"Enter a valid date for {_objectName} {childIndex}", invalidFields.ToArray());
            }
            else
            {
                return new ValidationResult($"Enter a valid date", invalidFields.ToArray());
            }
        }

        try
        {
            var dob = new DateTime(yearInt, monthInt, dayInt);

            if (dob > DateTime.Now)
            {
                if (childIndex != null)
                {
                    return new ValidationResult($"Enter a date in the past for {_objectName} {childIndex}", new[] { "DateOfBirth", "Day", "Month", "Year" });
                }
                else
                {
                    return new ValidationResult($"Enter a date in the past", new[] { "DateOfBirth", "Day", "Month", "Year" });
                }
            }

            if (dayInt > DateTime.DaysInMonth(yearInt, monthInt))
            {
                if (childIndex != null)
                {
                    return new ValidationResult($"Enter a valid day for {_objectName} {childIndex}", new[] { "DateOfBirth", "Day" });
                }
                else
                {
                    return new ValidationResult($"Enter a valid day", new[] { "DateOfBirth", "Day" });
                }
            }

            if (_applyAgeRange)
            {
                int age = CalculateAge(dob, DateTime.Now);

                if (age < 4 || age > 19)
                {
                    return new ValidationResult($"Enter an age between 4 and 19 for {_objectName} {childIndex}", new[] { "DateOfBirth", "Day", "Month", "Year" });
                }
            }
        }
        catch
        {
            if (childIndex != null)
            {
                return new ValidationResult($"Enter a valid {_fieldName} for child {childIndex}");
            }
            else
            {
                return new ValidationResult($"Enter a valid {_fieldName}");
            }
        }

        return ValidationResult.Success;
    }


    private string GetPropertyStringValue(object model, string propertyName)
    {
        return model.GetType().GetProperty(propertyName)?.GetValue(model) as string;
    }

    private int? GetPropertyIntValue(object model, string propertyName)
    {
        return model.GetType().GetProperty(propertyName)?.GetValue(model) as int?;
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