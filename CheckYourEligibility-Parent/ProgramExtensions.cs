using Microsoft.ApplicationInsights.Extensibility;
using CheckYourEligibility_FrontEnd.Telemetry;
using Microsoft.AspNetCore.Http;
using CheckYourEligibility_FrontEnd.Services;

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

            services.AddHttpClient<IEcsCheckService, EcsCheckService>(client =>
            {
                client.BaseAddress = new Uri(configuration["Api:Host"]);
            });

           
            services.AddHttpContextAccessor();

            
            services.AddSingleton<ITelemetryInitializer, ResponseBodyInitializer>();

            return services;
        }
    }
}
