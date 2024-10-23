using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_FrontEnd.Models
{
    public class School
    {
        public string? Name { get; set; }

        [Required(ErrorMessage = "School is required")]
        public string URN { get; set; }

        public string? LA { get; set; }

        public string? Postcode { get; set; }
    }
}
