using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_FrontEnd.Models
{
    public class ApplicationSearch
    {
        public int? LocalAuthority { get; set; }
        public int? School { get; set; }
        public ApplicationStatus? Status { get; set; }

        [LastName(ErrorMessage = "Child last name field contains an invalid character")]
        public string? ChildLastName { get; set; }

        [LastName(ErrorMessage = "Parent  or Guardian last name field contains an invalid character")]
        public string? ParentLastName { get; set; }

        [ReferenceNumber]
        public string? Reference { get; set; }

        [Range(1, 31, ErrorMessage = "Invalid day entered")]
        public int? ChildDOBDay { get; set; }

        [Range(1, 12, ErrorMessage = "Invalid month entered")]
        public int? ChildDOBMonth { get; set; }

        [Year]
        public int? ChildDOBYear { get; set; }

        [Range(1, 31, ErrorMessage = "Invalid Day")]
        public int? PGDOBDay { get; set; }

        [Range(1, 31, ErrorMessage = "Invalid Month")]
        public int? PGDOBMonth { get; set; }

        [Year]
        public int? PGDOBYear { get; set; }
        

    }
}
