using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility_FrontEnd.Services
{
    public interface IEcsServiceParent
    {
        Task<CheckEligibilityResponse> PostCheck(CheckEligibilityRequest requestBody);
        Task<CheckEligibilityStatusResponse> GetStatus(CheckEligibilityResponse responseBody);
        Task<SchoolSearchResponse> GetSchool(string name);
        Task<ApplicationSaveItemResponse> PostApplication(ApplicationRequest requestBody);
    }
}