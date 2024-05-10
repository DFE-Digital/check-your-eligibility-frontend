using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_Parent.Tests.Attributes.Derived;
using CheckYourEligibility_FrontEnd.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_Parent.Tests.Attributes
{
    public class NinoAttributeTests
    {
        const string NINOMIssingErrorMessage = "National Insurance Number is required";
        const string NINOFormatErrorMessage = "Invalid National Insurance Number format";

        private TestableNinoAttribute _ninoAttribute { get; set; }
        private ValidationContext _validationContext { get; set; }
        private Parent _parent { get; set; }

        [SetUp]
        public void Setup()
        {
            _parent = new Parent();
            _ninoAttribute = new TestableNinoAttribute();
            _validationContext = new ValidationContext(_parent);
        }


        [TestCase(null, NINOMIssingErrorMessage)]
        [TestCase("DB123456C", NINOFormatErrorMessage)]
        [TestCase("FB123456C", NINOFormatErrorMessage)]
        [TestCase("IB123456C", NINOFormatErrorMessage)]
        [TestCase("QB123456C", NINOFormatErrorMessage)]
        [TestCase("UB123456C", NINOFormatErrorMessage)]
        [TestCase("VB123456C", NINOFormatErrorMessage)]
        [TestCase("AD123456C", NINOFormatErrorMessage)]
        [TestCase("AF123456C", NINOFormatErrorMessage)]
        [TestCase("AI123456C", NINOFormatErrorMessage)]
        [TestCase("AQ123456C", NINOFormatErrorMessage)]
        [TestCase("AU123456C", NINOFormatErrorMessage)]
        [TestCase("AV123456C", NINOFormatErrorMessage)]
        [TestCase("AO123456C", NINOFormatErrorMessage)]
        [TestCase("BG123456C", NINOFormatErrorMessage)]
        [TestCase("GB123456C", NINOFormatErrorMessage)]
        [TestCase("KN123456C", NINOFormatErrorMessage)]
        [TestCase("NK123456C", NINOFormatErrorMessage)]
        [TestCase("NT123456C", NINOFormatErrorMessage)]
        [TestCase("TN123456C", NINOFormatErrorMessage)]
        [TestCase("ZZ123456C", NINOFormatErrorMessage)]
        [TestCase("ZZ123456C", NINOFormatErrorMessage)]
        [TestCase("AB123456E", NINOFormatErrorMessage)]
        public void CheckInvalidNINOs(string? nino, string? errorMessage)
        {
            var result = _ninoAttribute.NinoIsValid(nino, _validationContext);

            Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
        }

        [TestCase("ab123456c")]
        [TestCase("AB123456A")]
        [TestCase("AB123456C")]
        [TestCase("AB123456B")]
        [TestCase("AB123456C")]
        [TestCase("AB123456D")]
        public void CheckValidNINOs(string? nino)
        {
            var result = _ninoAttribute.NinoIsValid(nino, _validationContext);

            Assert.AreEqual(result, null);
        }
    }
}
