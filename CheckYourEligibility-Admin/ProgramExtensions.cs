
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CheckYourEligibility_FrontEnd.Services;

namespace CheckYourEligibility_FrontEnd
{
    public static class ProgramExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllersWithViews();
            
            var apiUrl = configuration["EcsBaseUrl"];
            if (Environment.GetEnvironmentVariable("KEY_VAULT_NAME")!=null&&Environment.GetEnvironmentVariable("KEY_VAULT_NAME")!="")
            {
                var keyVault = GetAzureKeyVault();

                apiUrl = keyVault.GetSecret("ApiUrl").Value.Value;
            }
            
            services.AddHttpClient<IEcsServiceParent, EcsServiceParent>(client =>
            {
                client.BaseAddress = new Uri(apiUrl);
            });
            
            return services;
        }

        private static SecretClient GetAzureKeyVault()
        {
            var keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
            var kvUri = $"https://{keyVaultName}.vault.azure.net";

            return new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
        }
    }
}
