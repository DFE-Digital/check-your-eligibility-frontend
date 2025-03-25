// Ignore Spelling: Fsm

using CheckYourEligibility.FrontEnd.Domain.Enums;

namespace CheckYourEligibility.FrontEnd.Boundary.Requests;

public class ApplicationStatusUpdateRequest
{
    public ApplicationStatusData? Data { get; set; }
}

public class ApplicationStatusData
{
    public ApplicationStatus Status { get; set; }
}