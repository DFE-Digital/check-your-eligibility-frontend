namespace CheckYourEligibility_DfeSignIn.PublicApi;

using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_DfeSignIn.PublicApi;
using Microsoft.Extensions.DependencyInjection;

public static class PublicApiExtensions
{
    /// <summary>
    /// Add support for the DfE sign-in public API.
    /// </summary>
    /// <remarks>
    /// <para>This is used to gather additional information such as user roles
    /// (see <see cref="IDfeSignInConfiguration.DiscoverRolesWithPublicApi"/>).</para>
    /// <para>This should be called before calling <see cref="AddDfeSignInAuthentication"/>
    /// when both services are being used.</para>
    /// </remarks>
    /// <param name="configuration">Configuration options.</param>
    /// <seealso cref="AddDfeSignInAuthentication"/>
    public static void AddDfeSignInPublicApi(this IServiceCollection services, IDfePublicApiConfiguration configuration)
    {
        services.AddSingleton<IDfePublicApiConfiguration>(configuration);
        services.AddSingleton<IDfePublicApi, DfePublicApi>();
    }
}
