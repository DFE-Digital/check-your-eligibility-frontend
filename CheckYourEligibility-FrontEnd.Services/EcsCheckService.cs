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
        private readonly string _FsmUrl;
        private readonly string _FsmbulkUploadUrl;

        public EcsCheckService(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration) : base("EcsService", logger, httpClient, configuration)
        {
            _logger = logger.CreateLogger("EcsService");
            _httpClient = httpClient;
            _FsmUrl = "FreeSchoolMeals";
            _FsmbulkUploadUrl = "FreeSchoolMeals/bulk";
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
                throw;
            }

        }

        public async Task<CheckEligibilityStatusResponse> GetStatus(CheckEligibilityResponse responseBody)
        {
            try
            {
                var response = await ApiDataGetAsynch($"{responseBody.Links.Get_EligibilityCheck}/Status", new CheckEligibilityStatusResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get Status failed. uri:-{_httpClient.BaseAddress}{responseBody.Links.Get_EligibilityCheck}/Status");
            }
            return null;
        }

        public async Task<CheckEligibilityBulkStatusResponse> GetBulkCheckProgress(string batchCheckUrl)
        {
            try
            {
                var result = await ApiDataGetAsynch(batchCheckUrl, new CheckEligibilityBulkStatusResponse());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"get failed. uri:-{_httpClient.BaseAddress}{_FsmbulkUploadUrl}");
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
                _logger.LogError(ex, $"get failed. uri:-{_httpClient.BaseAddress}{_FsmbulkUploadUrl}");
                throw;
            }
        }

        public async Task<CheckEligibilityResponseBulk> PostBulkCheck(CheckEligibilityRequestBulk requestBody)
        {
            try
            {
                var result = await ApiDataPostAsynch(_FsmbulkUploadUrl, requestBody, new CheckEligibilityResponseBulk());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Post failed. uri:-{_httpClient.BaseAddress}{_FsmbulkUploadUrl} content:-{JsonConvert.SerializeObject(requestBody)}");
                throw;
            }
        }
    }
}
