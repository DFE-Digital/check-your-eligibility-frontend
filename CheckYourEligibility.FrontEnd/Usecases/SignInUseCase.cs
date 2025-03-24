using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;

namespace CheckYourEligibility.FrontEnd.UseCases
{
    public interface ISignInUseCase
    {
        Task<AuthenticationProperties> Execute(string redirectUri);
    }

    public class SignInUseCase : ISignInUseCase
    {
        public Task<AuthenticationProperties> Execute(string redirectUri)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUri
            };

            // Setting the vector_of_trust directly in the Items dictionary
            properties.Items["vector_of_trust"] = @"[""Cl""]";

            return Task.FromResult(properties);
        }
    }
}