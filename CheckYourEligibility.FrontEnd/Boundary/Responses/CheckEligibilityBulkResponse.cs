namespace CheckYourEligibility.FrontEnd.Boundary.Responses;

public class CheckEligibilityBulkResponse
{
    public IEnumerable<CheckEligibilityItem> Data { get; set; }
}