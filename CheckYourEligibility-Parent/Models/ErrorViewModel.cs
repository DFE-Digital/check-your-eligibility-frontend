using System.Diagnostics.CodeAnalysis;

namespace CheckYourEligibility_FrontEnd.Models
{
    [ExcludeFromCodeCoverage(Justification = "Not depended on within solution/project")]
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
