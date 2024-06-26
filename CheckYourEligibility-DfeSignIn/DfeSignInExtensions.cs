
using System.Net;
using System.Security.Claims;
using CheckYourEligibility_DfeSignIn.Constants;
using CheckYourEligibility_DfeSignIn.Extensions;
using CheckYourEligibility_DfeSignIn.PublicApi;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        services.AddSingleton<IDfeSignInConfiguration>(configuration);

        services.AddHttpClient();
        services.AddHttpClient<DfePublicApi>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseDefaultCredentials = false,
                PreAuthenticate = true,
                Proxy = !string.IsNullOrWhiteSpace(configuration.APIServiceProxyUrl)
                    ? new WebProxy(new Uri(configuration.APIServiceProxyUrl, UriKind.Absolute))
                    : null
            });
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

                options.Events = new OpenIdConnectEvents()
                {
                    OnTokenValidated = async (context) =>
                    {
                        if (configuration.DiscoverRolesWithPublicApi)
                        {
                            await AddRoleClaimsFromDfePublicApi(context);
                        }
                    }
                };
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = configuration.CookieName;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(configuration.CookieExpireTimeSpanInMinutes);
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.SlidingExpiration = configuration.SlidingExpiration;
                options.AccessDeniedPath = configuration.AccessDeniedPath;
            });
    }

    private static async Task AddRoleClaimsFromDfePublicApi(TokenValidatedContext context)
    {
        var dfePublicApi = context.HttpContext.RequestServices.GetRequiredService<IDfePublicApi>();

        if (context.Principal?.Identity?.IsAuthenticated == true)
        {
            var userId = context.Principal.GetUserId();

            var userOrganization = context.Principal.GetOrganisation();
            if (userOrganization == null)
            {
                context.Fail("User is not in an organisation.");
                return;
            }

            var userAccessToService = await dfePublicApi.GetUserAccessToService(userId, userOrganization.Id.ToString());
            if (userAccessToService == null)
            {
                // User account is not enrolled into service and has no roles.
                return;
            }

            var roleClaims = new List<Claim>();
            foreach (var role in userAccessToService.Roles)
            {
                if (role.Status.Id.Equals(1))
                {
                    roleClaims.Add(new Claim(ClaimConstants.RoleCode, role.Code, ClaimTypes.Role, context.Options.ClientId));
                    roleClaims.Add(new Claim(ClaimConstants.RoleId, role.Id.ToString(), ClaimTypes.Role, context.Options.ClientId));
                    roleClaims.Add(new Claim(ClaimConstants.RoleName, role.Name, ClaimTypes.Role, context.Options.ClientId));
                    roleClaims.Add(new Claim(ClaimConstants.RoleNumericId, role.NumericId.ToString(), ClaimTypes.Role, context.Options.ClientId));
                }
            }

            var roleIdentity = new ClaimsIdentity(roleClaims);
            context.Principal.AddIdentity(roleIdentity);
        }
    }
}
