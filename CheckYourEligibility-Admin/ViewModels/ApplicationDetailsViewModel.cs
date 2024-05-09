using CheckYourEligibility_FrontEnd.Models;

namespace CheckYourEligibility_FrontEnd.ViewModels
{
    public class ApplicationDetailsViewModel
    {
        public Parent parentDetails { get; set; }

        public Child[] children { get; set; }
    }
}
