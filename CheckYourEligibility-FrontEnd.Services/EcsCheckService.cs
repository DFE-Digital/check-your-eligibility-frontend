using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.Services
{
    public class EcsCheckService : BaseService, IEcsCheckService
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly string _FsmCheckUrl;
        private readonly string _FsmCheckBulkUploadUrl;

        public EcsCheckService(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration) : base("EcsService", logger, httpClient, configuration)
        {
            _logger = logger.CreateLogger("EcsService");
            _httpClient = httpClient;
            _FsmCheckUrl = "check/free-school-meals"; 
            _FsmCheckBulkUploadUrl = "bulk/free-school-meals";
            
        }


        public async Task<CheckEligibilityResponse> PostCheck(CheckEligibilityRequest_Fsm requestBody)
        {
            try
            {
                var result = await ApiDataPostAsynch(_FsmCheckUrl, requestBody, new CheckEligibilityResponse());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Post Check failed. uri:-{_httpClient.BaseAddress}{_FsmCheckUrl} content:-{JsonConvert.SerializeObject(requestBody)}");
                throw;
            }
        }

        public async Task<CheckEligibilityStatusResponse> GetStatus(CheckEligibilityResponse responseBody)
        {
            try
            {
                var response = await ApiDataGetAsynch($"{responseBody.Links.Get_EligibilityCheck}/status", new CheckEligibilityStatusResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get Status failed. uri:-{_httpClient.BaseAddress}{responseBody.Links.Get_EligibilityCheck}/status");
            }
            return null;
        }

        public async Task<CheckEligibilityBulkStatusResponse> GetBulkCheckProgress(string bulkCheckUrl)
        {
            try
            {
                var result = await ApiDataGetAsynch(bulkCheckUrl, new CheckEligibilityBulkStatusResponse());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"get failed. uri:-{_httpClient.BaseAddress}{_FsmCheckBulkUploadUrl}");
            }
            return null;
        }

        public async Task<CheckEligibilityBulkResponse> GetBulkCheckResults(string resultsUrl)
        {
            try
            {
                var result = await ApiDataGetAsynch(resultsUrl, new CheckEligibilityBulkResponse());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"get failed. uri:-{_httpClient.BaseAddress}{_FsmCheckBulkUploadUrl}");
                throw;
            }
        }


        public async Task<CheckEligibilityResponseBulk> PostBulkCheck(CheckEligibilityRequestBulk_Fsm requestBody)
        {
            try
            {
                var result = await ApiDataPostAsynch(_FsmCheckBulkUploadUrl, requestBody, new CheckEligibilityResponseBulk());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Post failed. uri:-{_httpClient.BaseAddress}{_FsmCheckBulkUploadUrl} content:-{JsonConvert.SerializeObject(requestBody)}");
                throw;
            }
        }
    }
}
