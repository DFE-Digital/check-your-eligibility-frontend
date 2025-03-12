using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckYourEligibility_FrontEnd.Models
{
    public class Child
    {
        [NotMapped]
        public int ChildIndex { get; set; }

        [ChildName("first name")]
        public string? FirstName { get; set; }

        [ChildName("last name")]
        public string? LastName { get; set; }

        public School School { get; set; }
        
        [NotMapped]
        [Dob("date of birth", "child", "ChildIndex", "Day", "Month", "Year", isRequired: true, applyAgeRange: true)]
        public string? DateOfBirth { get; set; }

        public string? Day { get; set; }

        public string? Month { get; set; }

        public string? Year { get; set; }

    }
}
