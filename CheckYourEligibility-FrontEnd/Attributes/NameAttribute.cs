﻿using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CheckYourEligibility_FrontEnd.Attributes
{
    public class NameAttribute : ValidationAttribute
    {
        private static readonly string UnicodeOnlyPattern = @"^\p{L}+$";

        private static readonly Regex regex = new Regex(UnicodeOnlyPattern);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || value == "")
            {
                return ValidationResult.Success;
            }

            if (!regex.IsMatch(value.ToString()))
            {
                return new ValidationResult("Name field contains an invalid character");
            }

            return ValidationResult.Success;
        }
    }
}
