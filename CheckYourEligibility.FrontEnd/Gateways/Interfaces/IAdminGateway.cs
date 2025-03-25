using CheckYourEligibility.FrontEnd.Boundary.Requests;
using CheckYourEligibility.FrontEnd.Boundary.Responses;
using CheckYourEligibility.FrontEnd.Domain.Enums;

namespace CheckYourEligibility.FrontEnd.Gateways.Interfaces;

public interface IAdminGateway
{
    Task<ApplicationItemResponse> GetApplication(string id);
    Task<ApplicationSearchResponse> PostApplicationSearch(ApplicationRequestSearch requestBody);
    Task<ApplicationStatusUpdateResponse> PatchApplicationStatus(string id, ApplicationStatus status);
}