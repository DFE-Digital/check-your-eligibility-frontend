using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services.Domain;

namespace CheckYourEligibility_FrontEnd.Services
{
    public interface IEcsServiceAdmin
    {
        Task<ApplicationItemResponse> GetApplication(string id);
        Task<ApplicationSearchResponse> PostApplicationSearch(ApplicationRequestSearch2 requestBody);
        Task<ApplicationStatusUpdateResponse> PatchApplicationStatus(string id, ApplicationStatus status);
    }
}