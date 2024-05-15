using CheckYourEligibility.Domain;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Xml.Linq;

namespace CheckYourEligibility_FrontEnd.Services
{
    public class BaseService
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly TelemetryClient _telemetry;
        protected readonly IConfiguration _configuration;

        private static JwtAuthResponse _jwtAuthResponse;

        public BaseService(string serviceName, ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger.CreateLogger(serviceName);
            _httpClient = httpClient;
            _telemetry = new TelemetryClient();
            _configuration = configuration;

            Task.Run(Authorise).Wait();
        }

        public async Task Authorise()
        {
            try
            {
                if (_jwtAuthResponse == null || _jwtAuthResponse.Expires < DateTime.UtcNow)
                {
                    var url = $"{_httpClient.BaseAddress}api/Login";
                    var requestBody = new UserModel
                    {
                        Username = _configuration["Api:AuthorisationUsername"],
                        EmailAddress = _configuration["Api:AuthorisationEmail"],
                        Password = _configuration["Api:AuthorisationPassword"]
                    };
                    _jwtAuthResponse = await ApiDataPostAsynch(url, requestBody, new JwtAuthResponse());
                }

                _httpClient.DefaultRequestHeaders
                .Add("Authorization", "Bearer " + _jwtAuthResponse.Token);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Post Check failed. uri:-{_httpClient.BaseAddress}{url} content:-{JsonConvert.SerializeObject(requestBody)}");
            }

        }

        protected async Task<T2> ApiDataPostAsynch<T1, T2>(string address, T1 data, T2 result)
        {
            string uri = address;
            string json = JsonConvert.SerializeObject(data);
            HttpContent content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var task = await _httpClient.PostAsync(uri, content);
            if (task.IsSuccessStatusCode)
            {
                var jsonString = await task.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<T2>(jsonString);
            }
            else
            {
                var method = "POST";
                await LogApiError(task, method, uri, json);
                if (task.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
            }

            return result;
        }

        protected async Task<T> ApiDataDeleteAsynch<T>(string address, T result)
        {

            string uri = address;
            var task = await _httpClient.DeleteAsync(uri);
            if (task.IsSuccessStatusCode)
            {
                var jsonString = await task.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<T>(jsonString);
            }
            else
            {
                var method = "DELETE";
                await LogApiError(task, method, uri);
                if (task.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
            }
            return result;
        }

        protected async Task<T> ApiDataGetAsynch<T>(string address, T result)
        {
            string uri = address;

            var task = await _httpClient.GetAsync(uri);

            if (task.IsSuccessStatusCode)
            {
                var jsonString = await task.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<T>(jsonString);
            }
            else
            {
               
                var method = "GET";
                await LogApiError(task, method, uri);
                if (task.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
            }

            return result;
        }

        protected async Task<T2> ApiDataPutAsynch<T1, T2>(string address, T1 data, T2 result)
        {
            string uri = address;
            string json = JsonConvert.SerializeObject(data);
            HttpContent content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var task = await _httpClient.PutAsync(uri, content);
            if (task.IsSuccessStatusCode)
            {
                var jsonString = await task.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<T2>(jsonString);
            }
            else
            {
                var method = "PUT";
                await LogApiError(task, method, uri, json);
            }

            return result;
        }

        private async Task LogApiError(HttpResponseMessage task, string method, string uri, string data)
        {
            var guid = Guid.NewGuid().ToString();
            if (task.Content != null)
            {
                var jsonString = await task.Content.ReadAsStringAsync();
                _telemetry.TrackEvent($"API {method} failure",
                    new Dictionary<string, string>
                    {
                        {"LogId", guid},
                         {"Response Code", task.StatusCode.ToString()},
                        {"Address", uri},
                         {"Request Data", data},
                         {"Response Data", jsonString}
                    });
            }
            else
            {
                _telemetry.TrackEvent($"API Failure:-{method}",
                    new Dictionary<string, string> { { "LogId", guid }, { "Address", uri } });
            }
            throw new Exception($"API Failure:-{method} , your issue has been logged please use the following reference:- {guid}");
        }

        private async Task LogApiError(HttpResponseMessage task, string method, string uri)
        {
            var guid = Guid.NewGuid().ToString();


            if (task.Content != null)
            {
                var jsonString = await task.Content.ReadAsStringAsync();
                _telemetry.TrackEvent($"API {method} failure",
                    new Dictionary<string, string>
                    {
                        {"LogId", guid},
                        {"Address", uri},
                        {"Response Code", task.StatusCode.ToString()},
                        {"Data", jsonString}
                    });
            }
            else
            {
                _telemetry.TrackEvent($"API {method} failure",
                    new Dictionary<string, string> { { "LogId", guid }, { "Address", uri } });
            }
        }
    }
}
