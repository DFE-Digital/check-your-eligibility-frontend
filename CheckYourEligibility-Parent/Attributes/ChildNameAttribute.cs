﻿using CheckYourEligibility_FrontEnd.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CheckYourEligibility_FrontEnd.Attributes
{
    public class ChildNameAttribute : ValidationAttribute
    {
        private static readonly string UnicodeOnlyPattern = @"^[\p{L}\-']+$";
        private static readonly Regex regex = new Regex(UnicodeOnlyPattern);

        private readonly string _fieldName;

        public ChildNameAttribute(string fieldName)
        {
            _fieldName = fieldName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var child = validationContext.ObjectInstance as Child;

            if (child == null)
            {
                return new ValidationResult("Invalid child instance.");
            }

            var childIndex = child.ChildIndex;

            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult($"Enter a {_fieldName} for child {childIndex}");
            }

            if (!regex.IsMatch(value.ToString()))
            {
                return new ValidationResult($"Enter a {_fieldName} with valid characters for child {childIndex}");
            }

            return ValidationResult.Success;
        }
    }
}