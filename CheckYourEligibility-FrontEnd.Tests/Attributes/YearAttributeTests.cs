﻿using CheckYourEligibility_FrontEnd.Tests.Attributes.Derived;
using CheckYourEligibility_FrontEnd.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckYourEligibility_FrontEnd.Tests.Attributes
{
    public class YearAttributeTests
    {
        const string YearMissingErrorMessage = "Year is required";
        const string YearFormatErrorMessage = "Invalid Year";
        private TestableYearAttribute _yearAttribute { get; set; }
        private ValidationContext _validationContext { get; set; }
        private ParentDetailsViewModel _parentDetailsViewModel { get; set; }

        [SetUp]
        public void Setup()
        {
            _parentDetailsViewModel = new ParentDetailsViewModel();
            _yearAttribute = new TestableYearAttribute();
            _validationContext = new ValidationContext(_parentDetailsViewModel);
        }

        [TestCase(null, YearMissingErrorMessage)]
        [TestCase(1800, YearFormatErrorMessage)]
        [TestCase(2500, YearFormatErrorMessage)]
        public void CheckInvalidYears(int? year, string? errorMessage)
        {
            var result = _yearAttribute.YearIsValid(year, _validationContext);

            Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
        }

        [TestCase(2023)]
        [TestCase(1990)]
        [TestCase(1950)]
        public void CheckValidYearNumbers(int? year)
        {
            var result = _yearAttribute.YearIsValid(year, _validationContext);

            Assert.AreEqual(result, null);
        }
    }
}
