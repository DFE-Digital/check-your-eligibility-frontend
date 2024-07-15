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
        private readonly string _FsmbulkUploadUrl = "/FreeSchoolMeals/bulk";

        public EcsServiceAdmin(ILoggerFactory logger, HttpClient httpClient,IConfiguration configuration): base("EcsService", logger, httpClient, configuration)
        {
            _logger = logger.CreateLogger("EcsService");
            _httpClient = httpClient;
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
