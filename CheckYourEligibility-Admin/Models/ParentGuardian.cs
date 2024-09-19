using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckYourEligibility_FrontEnd.Models
{
    public class ParentGuardian
    {
        [Nino]
        [MaxLength(13)]
        public string? NationalInsuranceNumber { get; set; }

        [Nass]
        [MaxLength(10)]
        public string? NationalAsylumSeekerServiceNumber { get; set; }

        public bool IsNassSelected { get; set; }

        [Name]
        [Required(ErrorMessage = "Enter a first name")]
        public string? FirstName { get; set; }

        [Name]
        [Required(ErrorMessage = "Enter a last name")]
        public string? LastName { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Enter an email")]
        public string? EmailAddress { get; set; }

        [NotMapped]
        [Dob("Day", "Month", "Year", isRequired: true)]
        public string? DateOfBirth { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a day using numbers only")]
        public string? Day { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a Month using numbers only")]
        public string? Month { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a Year using numbers only")]
        public string? Year { get; set; }
    }
}
