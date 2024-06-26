﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace CheckYourEligibility_DfeSignIn;
public static class DfeSignInExtensions
{
    /// <summary>
    /// Add support for DfE sign-in authentication using Open ID.
    /// </summary>
    /// <param name="configuration">Configuration options.</param>
    /// <seealso cref="AddDfeSignInPublicApi"/>
    public static void AddDfeSignInAuthentication(this IServiceCollection services, IDfeSignInConfiguration configuration)
    {
        services.AddSingleton(configuration);

        services.AddHttpClient();
        services.AddHttpContextAccessor();

        services.AddAuthentication(sharedOptions =>
        {
            sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
            .AddOpenIdConnect(options =>
            {
                options.ClientId = configuration.ClientId;
                options.ClientSecret = configuration.ClientSecret;
               
                options.Authority = configuration.Authority;
                options.MetadataAddress = configuration.MetaDataUrl;
                

                options.CallbackPath = new PathString(configuration.CallbackUrl);
                options.SignedOutRedirectUri = new PathString(configuration.SignoutRedirectUrl);
                options.SignedOutCallbackPath = new PathString(configuration.SignoutCallbackUrl);
                options.ResponseType = OpenIdConnectResponseType.Code;

                options.Scope.Clear();
                foreach (string scope in configuration.Scopes)
                {
                    options.Scope.Add(scope);
                }

                options.GetClaimsFromUserInfoEndpoint = configuration.GetClaimsFromUserInfoEndpoint;
                options.SaveTokens = configuration.SaveTokens;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = configuration.CookieName;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(configuration.CookieExpireTimeSpanInMinutes);
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.SlidingExpiration = configuration.SlidingExpiration;
            });
    }
}
