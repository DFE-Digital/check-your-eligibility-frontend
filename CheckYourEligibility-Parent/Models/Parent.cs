using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public bool? IsNinoSelected { get; set; }

        [Name]
        [Required(ErrorMessage = "First Name is required")]
        public string? FirstName { get; set; }

        [Name]
        [Required(ErrorMessage = "Last Name is required")]
        public string? LastName { get; set; }

        [NotMapped]
        [Dob]
        public DateTime? DateOfBirth { get; set; }

        public int? Day { get; set; }

        public int? Month { get; set; }

        public int? Year { get; set; }

        public bool NASSRedirect { get; set; }

    }
}
