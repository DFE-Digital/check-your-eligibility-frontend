using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility_FrontEnd.Services
{
    public interface IEcsServiceAdmin
    {
        Task<ApplicationSearchResponse> PostApplicationSearch(ApplicationRequestSearch requestBody);
    }
}