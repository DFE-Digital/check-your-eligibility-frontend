using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace CheckYourEligibility_FrontEnd.Telemetry

{
    public class ResponseBodyInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResponseBodyInitializer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Response?.Body != null && context.Response.Body.CanSeek)
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(context.Response.Body, leaveOpen: true))
                {
                    string responseBody = reader.ReadToEnd();

                    // Optional: Truncate if too long
                    if (responseBody.Length > 1000)
                    {
                        responseBody = responseBody.Substring(0, 1000) + "...";
                    }

                    // Add the response body to telemetry
                    if (telemetry is Microsoft.ApplicationInsights.DataContracts.RequestTelemetry requestTelemetry)
                    {
                        requestTelemetry.Properties["ResponseBody"] = responseBody;
                    }

                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                }
            }
        }
    }
}
