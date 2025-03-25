using System.ComponentModel.DataAnnotations;
using CheckYourEligibility.FrontEnd.Boundary.Responses;

namespace CheckYourEligibility.FrontEnd.ViewModels;

public class SchoolListViewModel
{
    public List<Establishment>? Schools { get; set; }

    [Required(ErrorMessage = "Select yes if any of your children go to these schools")]
    public bool? IsRadioSelected { get; set; }
}