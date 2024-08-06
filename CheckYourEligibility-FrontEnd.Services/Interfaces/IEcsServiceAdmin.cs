using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility_FrontEnd.Services
{
    public interface IEcsServiceAdmin
    {
        Task<ApplicationItemResponse> GetApplication(string id);
        Task<ApplicationSearchResponse> PostApplicationSearch(ApplicationRequestSearch requestBody);
    }
}