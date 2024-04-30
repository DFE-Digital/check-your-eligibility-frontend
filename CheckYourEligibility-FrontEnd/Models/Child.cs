using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_FrontEnd.Models
{
    public class Child
    {
        [Name]
        [Required(ErrorMessage = "First name is required")]
        public string? FirstName { get; set; }

        [Name]
        [Required(ErrorMessage = "Last name is required")]
        public string? LastName { get; set; }

        [Dob]
        [Required(ErrorMessage = "Day is required")]
        [Range(1, 31, ErrorMessage = "Invalid Day")]
        public int? Day { get; set; }

        [Required(ErrorMessage = "Month is required")]
        [Range(1, 12, ErrorMessage = "Invalid Month")]
        public int? Month { get; set; }

        [Year]
        [Required(ErrorMessage = "Year is required")]
        public int? Year { get; set; }

        public School school { get; set; }
    }
}
