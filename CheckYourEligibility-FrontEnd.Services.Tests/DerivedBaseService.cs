using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace CheckYourEligibility_FrontEnd.Services.Tests.Parent
{
    public class EcsServiceParentTest : EcsServiceParent
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
