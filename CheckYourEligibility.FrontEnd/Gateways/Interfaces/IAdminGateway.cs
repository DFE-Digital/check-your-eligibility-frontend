using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility.FrontEnd.Gateways.Interfaces
{
    public interface IAdminGateway
    {
        Task<ApplicationItemResponse> GetApplication(string id);
        Task<ApplicationSearchResponse> PostApplicationSearch(ApplicationRequestSearch requestBody);
        Task<ApplicationStatusUpdateResponse> PatchApplicationStatus(string id, ApplicationStatus status);
    }
}