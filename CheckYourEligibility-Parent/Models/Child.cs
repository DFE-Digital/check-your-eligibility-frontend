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
        public DateTime? DateOfBirth { get; set; }

        public int? Day { get; set; }

        public int? Month { get; set; }

        public int? Year { get; set; }

        public School School { get; set; }
    }
}
