using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;

namespace CheckYourEligibility_Admin.UseCases
{
    public interface ISignInUseCase
    {
        Task<AuthenticationProperties> Execute(string redirectUri);
    }

    public class AdminSignInUseCase : ISignInUseCase
    {
        public Task<AuthenticationProperties> Execute(string redirectUri)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUri
            };

            properties.Items["vector_of_trust"] = @"[""Cl""]";

            return Task.FromResult(properties);
        }
    }
}