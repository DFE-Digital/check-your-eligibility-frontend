using CheckYourEligibility.FrontEnd.Boundary.Requests;
using CheckYourEligibility.FrontEnd.Boundary.Responses;

namespace CheckYourEligibility.FrontEnd.Gateways.Interfaces;

public interface IParentGateway
{
    Task<EstablishmentSearchResponse> GetSchool(string name);

    Task<UserSaveItemResponse> CreateUser(UserCreateRequest requestBody);

    Task<ApplicationSaveItemResponse> PostApplication_Fsm(ApplicationRequest requestBody);
}