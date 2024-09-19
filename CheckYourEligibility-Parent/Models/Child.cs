using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [NotMapped]
        [Dob]
        public string? DateOfBirth { get; set; }

        public string? Day { get; set; }

        public string? Month { get; set; }

        public string? Year { get; set; }

        public School School { get; set; }
    }
}
