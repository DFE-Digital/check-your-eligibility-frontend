﻿using System.ComponentModel.DataAnnotations;
using CheckYourEligibility.FrontEnd.Attributes;

namespace CheckYourEligibility.FrontEnd.Tests.Attributes.Derived;

public class TestableNameAttribute : NameAttribute
{
    public ValidationResult NameIsValid(object value, ValidationContext validationContext)
    {
        return IsValid(value, validationContext);
    }
}