using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace CheckYourEligibility_FrontEnd.Services
{
    public class EcsServiceAdmin : BaseService,  IEcsServiceAdmin
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly string _ApplicationSearchUrl = "/FreeSchoolMeals/Application/Search";
        private readonly string _ApplicationUrl = "/FreeSchoolMeals/Application";

        public EcsServiceAdmin(ILoggerFactory logger, HttpClient httpClient,IConfiguration configuration): base("EcsService", logger, httpClient, configuration)
        {
            _logger = logger.CreateLogger("EcsService");
            _httpClient = httpClient;       
        }

       
        public async Task<ApplicationItemResponse> GetApplication(string id)
        {
            try
            {
                var response = await ApiDataGetAsynch($"{_httpClient.BaseAddress}{_ApplicationUrl}/{id}", new ApplicationItemResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get failed. uri-{_httpClient.BaseAddress}{_ApplicationUrl}/{id}");
                throw;
            }
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
                _logger.LogError(ex, $"Post failed. uri:-{_httpClient.BaseAddress}{_ApplicationSearchUrl} content:-{JsonConvert.SerializeObject(requestBody)}");
                throw;
            }
        }
    }
}
