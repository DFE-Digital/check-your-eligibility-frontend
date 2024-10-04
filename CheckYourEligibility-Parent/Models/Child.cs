using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckYourEligibility_FrontEnd.Models
{
    public class Child
    { 
        [Name]
        [Required(ErrorMessage = "Enter child's first name")]
        public string? FirstName { get; set; }

        [Name]
        [Required(ErrorMessage = "Enter child's last name")]
        public string? LastName { get; set; }
        
        public School School { get; set; }
        
        [NotMapped]
        [Dob]
        public string? DateOfBirth { get; set; }

        public string? Day { get; set; }

        public string? Month { get; set; }

        public string? Year { get; set; }

    }
}
