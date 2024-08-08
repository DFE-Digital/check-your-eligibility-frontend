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
        private readonly string _FsmUrl;
        private readonly string _schoolUrl;

        public  EcsServiceParent(ILoggerFactory logger, HttpClient httpClient,IConfiguration configuration): base("EcsService", logger, httpClient, configuration)
        {
            _logger = logger.CreateLogger("EcsService");
            _httpClient = httpClient;
            _FsmUrl = "FreeSchoolMeals";
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

        public async Task<ApplicationSaveItemResponse> PostApplication(ApplicationRequest requestBody)
        {
            try
            {
                var response = await ApiDataPostAsynch($"{_FsmUrl}/Application", requestBody, new ApplicationSaveItemResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Post Application failed. uri-{_httpClient.BaseAddress}{_FsmUrl}/Application");
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
