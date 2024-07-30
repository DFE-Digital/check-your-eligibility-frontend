using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility_FrontEnd.Services.Tests
{
    internal class DerivedCheckService : EcsCheckService
    {

        public int apiErrorCount { get; private set; }

        public DerivedCheckService(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration)
            : base(logger, httpClient, configuration)
        {
            apiErrorCount = 0;
        }

        protected override Task LogApiErrorInternal(HttpResponseMessage task, string method, string uri, string data)
        {
            apiErrorCount++;
            return Task.CompletedTask;
        }

        protected override Task LogApiErrorInternal(HttpResponseMessage task, string method, string uri)
        {
            apiErrorCount++;
            return Task.CompletedTask;
        }
    }
}
