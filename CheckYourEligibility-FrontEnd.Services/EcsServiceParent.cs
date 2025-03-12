using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility_FrontEnd.Services
{
    public class EcsServiceParent : BaseService, IEcsServiceParent
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly string _ApplicationUrl;
        private readonly string _schoolUrl;

        public EcsServiceParent(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration)
            : base("EcsService", logger, httpClient, configuration)
        {
            _logger = logger.CreateLogger("EcsService");
            _httpClient = httpClient;
            _ApplicationUrl = "application";
            _schoolUrl = "establishment";
        }

        public async Task<EstablishmentSearchResponse> GetSchool(string name)
        {
            try
            {
                var sanitizedQuery = name?.Replace(Environment.NewLine, "")
                                        .Replace("\n", "")
                                        .Replace("\r", "");

                var requestUrl = $"{_schoolUrl}/search?query={Uri.EscapeDataString(name)}";
                var response = await ApiDataGetAsynch(requestUrl, new EstablishmentSearchResponse());

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "School search failed");
                throw;
            }
        }

        public async Task<ApplicationSaveItemResponse> PostApplication_Fsm(ApplicationRequest requestBody)
        {
            try
            {
                requestBody.Data.Type = CheckYourEligibility.Domain.Enums.CheckEligibilityType.FreeSchoolMeals;
                var response = await ApiDataPostAsynch($"{_ApplicationUrl}", requestBody, new ApplicationSaveItemResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post Application failed for FSM application");
                throw;
            }
        }

        public async Task<UserSaveItemResponse> CreateUser(UserCreateRequest requestBody)
        {
            try
            {
                var response = await ApiDataPostAsynch("user", requestBody, new UserSaveItemResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create User failed");
            }
            return null;
        }
    }
}
