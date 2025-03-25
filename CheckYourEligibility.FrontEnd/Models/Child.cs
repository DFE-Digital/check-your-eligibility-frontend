using System.ComponentModel.DataAnnotations.Schema;
using CheckYourEligibility.FrontEnd.Attributes;

namespace CheckYourEligibility.FrontEnd.Models;

public class Child
{
    [NotMapped] public int ChildIndex { get; set; }

    [ChildName("first name")] public string? FirstName { get; set; }

    [ChildName("last name")] public string? LastName { get; set; }

    public School School { get; set; }

    [NotMapped]
    [Dob("date of birth", "child", "ChildIndex", "Day", "Month", "Year", true, true)]
    public string? DateOfBirth { get; set; }

    public string? Day { get; set; }

    public string? Month { get; set; }

    public string? Year { get; set; }
}