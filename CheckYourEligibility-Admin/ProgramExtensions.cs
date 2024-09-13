using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.ApplicationInsights.Extensibility;
using CheckYourEligibility_FrontEnd.Telemetry; // Ensure this namespace is correct
using Microsoft.AspNetCore.Http;

namespace CheckYourEligibility_FrontEnd
{
    public static class ProgramExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllersWithViews();

            services.AddHttpClient<IEcsServiceParent, EcsServiceParent>(client =>
            {
                client.BaseAddress = new Uri(configuration["Api:Host"]);
            });

            services.AddHttpClient<IEcsServiceAdmin, EcsServiceAdmin>(client =>
            {
                client.BaseAddress = new Uri(configuration["Api:Host"]);
            });

            services.AddHttpClient<IEcsCheckService, EcsCheckService>(client =>
            {
                client.BaseAddress = new Uri(configuration["Api:Host"]);
            });

            // Register IHttpContextAccessor
            services.AddHttpContextAccessor();

            // Register the Telemetry Initializer
            services.AddSingleton<ITelemetryInitializer, ResponseBodyInitializer>();

            return services;
        }
    }
}
