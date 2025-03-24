using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using CheckYourEligibility.FrontEnd.Gateways;

namespace CheckYourEligibility.FrontEnd.Services.Tests.Parent
{
    public class EcsServiceParentTest : ParentGateway
    {
        public int apiErrorCount { get; private set; }

        public EcsServiceParentTest(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration)
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
