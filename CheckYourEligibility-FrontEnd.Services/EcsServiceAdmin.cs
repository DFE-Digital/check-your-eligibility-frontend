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
        private readonly string _FsmbulkUploadUrl = "/FreeSchoolMeals/bulk";

        public EcsServiceAdmin(ILoggerFactory logger, HttpClient httpClient,IConfiguration configuration): base("EcsService", logger, httpClient, configuration)
        {
            _logger = logger.CreateLogger("EcsService");
            _httpClient = httpClient;
            _FsmUrl = "/FreeSchoolMeals";
            _schoolUrl = "/Schools";
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
            }
            return null;
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
            }
            return null;
        }
    }
}
