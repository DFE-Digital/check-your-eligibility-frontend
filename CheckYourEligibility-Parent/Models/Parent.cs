using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_FrontEnd.Models
{
    public class Parent
    {
        [Nino]
        [MaxLength(13)]
        public string? NationalInsuranceNumber { get; set; }

        [Nass]
        [MaxLength(10)]
        public string? NationalAsylumSeekerServiceNumber { get; set; }

        public bool? IsNassSelected { get; set; }

        [NinoSelection]
        public bool? IsNinoSelected { get; set; }

        [Name]
        [Required(ErrorMessage = "Enter a first name")]
        public string? FirstName { get; set; }

        [Name]
        [Required(ErrorMessage = "Enter a last name")]
        public string? LastName { get; set; }

        [Dob]
        [Required(ErrorMessage = "Enter a valid day")]
        [Range(1, 31, ErrorMessage = "Enter a valid day")]
        public int? Day { get; set; }

        [Required(ErrorMessage = "Enter a valid month")]
        [Range(1, 12, ErrorMessage = "Enter a valid month")]
        public int? Month { get; set; }

        [Year]
        [Required(ErrorMessage = "Enter a valid year")]
        public int? Year { get; set; }

        public bool NASSRedirect { get; set; }
    }
}
