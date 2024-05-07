using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace CheckYourEligibility_FrontEnd.Services
{
    public class BaseService
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly TelemetryClient _telemetry;
        public BaseService(string serviceName, ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger.CreateLogger(serviceName);
            _httpClient = httpClient;
            _telemetry = new TelemetryClient();
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
            }
            return result;
        }

        protected async Task<T> ApiDataGetAsynch<T>(string address, T result)
        {
            // add bearer token temp
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlY3NVaVVzZXIiLCJlbWFpbCI6ImVjc1VpQGVkdWNhdGlvbi5nb3YudWsiLCJFY3NBcGkiOiJhcGlDdXN0b21DbGFpbSIsImp0aSI6IjBiMmZiZTBmLWM2OGUtNDI0MC05YjY3LTExMzczMGQ4N2EzNiIsImV4cCI6MTcxNTA5NTgyOSwiaXNzIjoiZWNzLmNvbSIsImF1ZCI6ImVjcy5jb20ifQ.A6tt0L-HhlywWoEq-RefHZrhgC0qzKATpcRAHEiloiE");
            string uri = address;

            var task = await _httpClient.GetAsync(uri);
            if (task.IsSuccessStatusCode)
            {
                var jsonString = await task.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<T>(jsonString);
            }
            else
            {
                if (task.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                var method = "GET";
                await LogApiError(task, method, uri);
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
