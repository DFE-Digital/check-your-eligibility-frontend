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
    public class EcsService : BaseService,  IEcsService
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly string _FsmUrl;

        public EcsService(ILoggerFactory logger, HttpClient httpClient,IConfiguration configuration): base("EcsService", logger, httpClient, configuration)
        {
            _logger = logger.CreateLogger("EcsService");
            _httpClient = httpClient;
            _FsmUrl = configuration["EcsFsmControllerUrl"];
        }

        public async Task<CheckEligibilityResponse> PostCheck(CheckEligibilityRequest requestBody)
        {
            try
            {
                var result = await ApiDataPostAsynch(_FsmUrl, requestBody, new CheckEligibilityResponse());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Post Check failed. uri:-{_httpClient.BaseAddress}{_FsmUrl} content:-{JsonConvert.SerializeObject(requestBody)}");
            }
            return null;
        }
       
        public async Task<StatusResponse> GetStatus(CheckEligibilityResponse responseBody)
        {
            try
            {
                var response = await ApiDataGetAsynch($"{responseBody.Links.Get_EligibilityCheck}/Status", new StatusResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get Status failed. uri:-{_httpClient.BaseAddress}{responseBody.Links.Get_EligibilityCheck}/Status");
            }
            return null;
        }
    }
}
