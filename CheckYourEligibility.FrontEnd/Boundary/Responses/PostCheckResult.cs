using CheckYourEligibility.FrontEnd.Domain.Enums;

namespace CheckYourEligibility.FrontEnd.Boundary.Responses;

public class PostCheckResult
{
    public string Id { get; set; }
    public CheckEligibilityStatus Status { get; set; }
}