using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckYourEligibility_FrontEnd.Models
{
    public class School
    {
        [NotMapped]  // not ideal to have this property on the model, but currently necessary to build dynamic error messages on the UI
        public int ChildIndex { get; set; }

        public string? Name { get; set; }

        [School("ChildIndex")]
        public string? URN { get; set; }

        public string? LA { get; set; }

        public string? Postcode { get; set; }
    }
}
