using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility_FrontEnd.Services
{
    public interface IEcsServiceParent
    {
        Task<SchoolSearchResponse> GetSchool(string name);
        Task<ApplicationSaveItemResponse> PostApplication(ApplicationRequest requestBody);
        Task<UserSaveItemResponse> CreateUser(UserCreateRequest requestBody);
    }
}