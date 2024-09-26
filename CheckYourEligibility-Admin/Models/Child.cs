using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckYourEligibility_FrontEnd.Models
{
    public class Child
    { 
        [Name]
        [Required(ErrorMessage = "Enter a child's first name")]
        public string? FirstName { get; set; }

        [Name]
        [Required(ErrorMessage = "Enter a child's last name")]
        public string? LastName { get; set; }

        [NotMapped]
        [Dob("Day", "Month", "Year", isRequired: true, applyAgeRange: true)]
        public string? DateOfBirth { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a day using numbers only")]
        public string? Day { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a Month using numbers only")]
        public string? Month { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a Year using numbers only")]
        public string? Year { get; set; }

        public School? School { get; set; }
    }
}
