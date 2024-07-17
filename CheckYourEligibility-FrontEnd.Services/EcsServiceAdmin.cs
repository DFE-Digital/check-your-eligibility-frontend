using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;

namespace CheckYourEligibility_FrontEnd.Services
{
    public class EcsServiceAdmin : BaseService,  IEcsServiceAdmin
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly string _FsmUrl;
        private readonly string _schoolUrl;
        private readonly string _ApplicationSearchUrl;

        public EcsServiceAdmin(ILoggerFactory logger, HttpClient httpClient,IConfiguration configuration): base("EcsService", logger, httpClient, configuration)
        {
            _logger = logger.CreateLogger("EcsService");
            _httpClient = httpClient;
            _FsmUrl = "/FreeSchoolMeals";
            _schoolUrl = "/Schools";
            _ApplicationSearchUrl = "/FreeSchoolMeals/Application/Search";
        }

        public async Task<ApplicationSearchResponse> ApplicationSearch(ApplicationRequestSearch requestBody)
        {
            try
            {
                var result = await ApiDataPostAsynch(_ApplicationSearchUrl, requestBody, new ApplicationSearchResponse());
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Post Check failed. uri:-{_httpClient.BaseAddress}{_ApplicationSearchUrl} content:-{JsonConvert.SerializeObject(requestBody)}");
            }
            return null;
        }
    }
}
