using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility_FrontEnd.Services
{
    public class EcsServiceParent : BaseService,  IEcsServiceParent
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly string _ApplicationUrl;
        private readonly string _schoolUrl;

        public  EcsServiceParent(ILoggerFactory logger, HttpClient httpClient,IConfiguration configuration): base("EcsService", logger, httpClient, configuration)
        {
            _logger = logger.CreateLogger("EcsService");
            _httpClient = httpClient;
            _ApplicationUrl = "Application";
            _schoolUrl = "Schools";
        }

        public async Task<SchoolSearchResponse> GetSchool(string name)
        {
            try
            {
                var response = await ApiDataGetAsynch($"{_httpClient.BaseAddress}{_schoolUrl}/Search?query={name}", new SchoolSearchResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get School failed. uri-{_httpClient.BaseAddress}{_schoolUrl}/Search?query={name}");
                throw;
            }
        }

        public async Task<ApplicationSaveItemResponse> PostApplication_Fsm(ApplicationRequest requestBody)
        {
            try
            {
                requestBody.Data.Type = CheckYourEligibility.Domain.Enums.CheckEligibilityType.FreeSchoolMeals;
                var response = await ApiDataPostAsynch($"{_ApplicationUrl}/FreeSchoolMeals", requestBody, new ApplicationSaveItemResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Post Application failed. uri-{_httpClient.BaseAddress}{_ApplicationUrl}");
                throw;
            }
        }

        public async Task<UserSaveItemResponse> CreateUser(UserCreateRequest requestBody)
        {
            try
            {
                var response = await ApiDataPostAsynch("Users", requestBody, new UserSaveItemResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Create User failed. uri-{_httpClient.BaseAddress}Users");
            }

            return null;
        }
    }
}
