using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.Services
{
    public class EcsServiceAdmin : BaseService,  IEcsServiceAdmin
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly string _ApplicationSearchUrl;

        public EcsServiceAdmin(ILoggerFactory logger, HttpClient httpClient,IConfiguration configuration): base("EcsService", logger, httpClient, configuration)
        {
            _logger = logger.CreateLogger("EcsService");
            _httpClient = httpClient;
            _ApplicationSearchUrl = "/FreeSchoolMeals/Application/Search";
        }

        public async Task<ApplicationSearchResponse> PostApplicationSearch(ApplicationRequestSearch requestBody)
        {
            try
            {
                var result = await ApiDataPostAsynch(_ApplicationSearchUrl, requestBody, new ApplicationSearchResponse());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Post Check failed. uri:-{_httpClient.BaseAddress}{_ApplicationSearchUrl} content:-{JsonConvert.SerializeObject(requestBody)}");
            }
            return null;
        }
    }
}
