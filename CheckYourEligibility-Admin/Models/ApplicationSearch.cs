using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckYourEligibility_FrontEnd.Models
{
    public class ApplicationSearch
    {
        // Pagination Properties
        public int PageNumber { get; set; } = 1; // Default to page 1
        public int PageSize { get; set; } = 10; // Default to 10 items per page
        //
        public int? LocalAuthority { get; set; }
        public int? School { get; set; }
        public ApplicationStatus? Status { get; set; }

        [LastName(ErrorMessage = "Child last name field contains an invalid character")]
        public string? ChildLastName { get; set; }

        [LastName(ErrorMessage = "Parent or Guardian last name field contains an invalid character")]
        public string? ParentLastName { get; set; }

        [ReferenceNumber]
        public string? Reference { get; set; }

        [NotMapped]
        [Dob("ChildDobDay", "ChildDobMonth", "ChildDobYear", isRequired: false)]
        public string? ChildDob { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a day using numbers only")]
        public int? ChildDobDay { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a month using numbers only")]
        public int? ChildDobMonth { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a year using numbers only")]
        public int? ChildDobYear { get; set; }

        [NotMapped]
        [Dob("PGDobDay", "PGDobMonth", "PGDobYear", isRequired: false)]
        public string? ParentDob { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a day using numbers only")]
        public int? PGDobDay { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a month using numbers only")]
        public int? PGDobMonth { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a year using numbers only")]
        public int? PGDobYear { get; set; }
        

    }
}
