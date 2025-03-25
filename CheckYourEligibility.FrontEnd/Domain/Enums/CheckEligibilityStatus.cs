// Ignore Spelling: Fsm

namespace CheckYourEligibility.FrontEnd.Domain.Enums;

public enum CheckEligibilityStatus
{
    queuedForProcessing,
    parentNotFound,
    eligible,
    notEligible,
    error
}