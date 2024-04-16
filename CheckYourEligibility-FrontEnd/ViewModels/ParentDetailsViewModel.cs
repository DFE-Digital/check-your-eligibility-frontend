using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_FrontEnd.ViewModels
{
    public class ParentDetailsViewModel
    {
        [Nino]
        public string? NationalInsuranceNumber { get; set; }

        public string? NationalAsylumSeekerServiceNumber { get; set; }

        public bool IsNassSelected { get; set; }

        [Name]
        [Required(ErrorMessage = "First Name is required")]
        public string? FirstName { get; set; }

        [Name]
        [Required(ErrorMessage = "Last Name is required")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Day is required")]
        [Range(1, 31, ErrorMessage = "Invalid Day")]
        public int? Day { get; set; }

        [Required(ErrorMessage = "Month is required")]
        [Range(1, 12, ErrorMessage = "Invalid Month")]
        public int? Month { get; set; }

        [Year]
        [Required(ErrorMessage = "Year is required")]
        public int? Year { get; set; }
    }
}
