using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_FrontEnd.ViewModels
{
    public class SchoolListViewModel
    {
        public List<CheckYourEligibility.Domain.Responses.Establishment>? Schools { get; set; }

        [Required(ErrorMessage = "Select yes if any of your children go to these schools")]
        public bool? IsRadioSelected { get; set; }
    }

}
