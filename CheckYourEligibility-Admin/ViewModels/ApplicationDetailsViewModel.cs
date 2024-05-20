using CheckYourEligibility_FrontEnd.Models;

namespace CheckYourEligibility_FrontEnd.ViewModels
{
    public class ApplicationDetailsViewModel
    {
        public ParentGuardian parentDetails { get; set; }

        public Child[] children { get; set; }
    }
}
