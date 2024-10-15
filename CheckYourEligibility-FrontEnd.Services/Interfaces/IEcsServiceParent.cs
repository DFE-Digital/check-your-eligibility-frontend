using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility_FrontEnd.Services
{
    public interface IEcsServiceParent
    {
        Task<SchoolSearchResponse> GetSchool(string name);
       
        Task<UserSaveItemResponse> CreateUser(UserCreateRequest requestBody);

        Task<ApplicationSaveItemResponse> PostApplication_Fsm(ApplicationRequest requestBody);
    }
}