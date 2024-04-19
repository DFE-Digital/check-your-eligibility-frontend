using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace CheckYourEligibility_FrontEnd.Services
{
    public class EcsService : IEcsService
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly string _FsmUrl;

        public EcsService(ILoggerFactory logger, HttpClient httpClient,IConfiguration configuration)
        {
            _logger = logger.CreateLogger("EcsService");
            _httpClient = httpClient;
            _FsmUrl = configuration["EcsFsmControllerUrl"];
        }

        public async Task<CheckEligibilityResponse> PostCheck(CheckEligibilityRequest requestBody)
        {
            var uri = $"{_FsmUrl}";
            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonConvert.DeserializeObject<CheckEligibilityResponse>(response.Content.ReadAsStringAsync().Result);
                    return responseData;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var ErrorResponseData = JsonConvert.DeserializeObject<MessageResponse>(response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Post Check failed. uri:-{_httpClient.BaseAddress}{uri} content:-{JsonConvert.SerializeObject(requestBody)}");
            }
            return null;
        }

        public async Task<StatusResponse> GetStatus(CheckEligibilityResponse responseBody)
        {
            var request = $"{responseBody.Links.Get_EligibilityCheck}/Status";
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonConvert.DeserializeObject<StatusResponse>(response.Content.ReadAsStringAsync().Result);
                    return responseData;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var ErrorResponseData = JsonConvert.DeserializeObject<MessageResponse>(response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get Check failed: uri:-{_httpClient.BaseAddress}{request} content-{JsonConvert.SerializeObject(responseBody)}");
            }
            return null;
        }
    }
}
