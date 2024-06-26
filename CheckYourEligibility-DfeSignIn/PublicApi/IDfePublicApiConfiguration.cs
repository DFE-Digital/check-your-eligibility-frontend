using CheckYourEligibility_DfeSignIn.PublicApi;

namespace CheckYourEligibility_DfeSignIn.PublicApi;

/// <summary>
/// Configuration for the <see cref="DfePublicApi"/> service implementation.
/// </summary>
public interface IDfePublicApiConfiguration
{
    /// <summary>
    /// Gets the environment specific base URL for the DfE sign-in public API.
    /// </summary>
    string BaseUrl { get; }

    /// <summary>
    /// Gets the secret which is required for interacting with the API.
    /// </summary>
    /// <remarks>
    /// <para>This should reflect the configuration in the DfE Sign-in "Manage service"
    /// web interface.</para>
    /// </remarks>
    string ApiSecret { get; }

    /// <summary>
    /// Gets the client ID of the service.
    /// </summary>
    /// <remarks>
    /// <para>This should reflect the configuration in the DfE Sign-in "Manage service"
    /// web interface.</para>
    /// </remarks>
    string ClientId { get; }

    /// <summary>
    /// Gets the service audience which is typically "signin.education.gov.uk".
    /// </summary>
    /// <remarks>
    /// <para>
    /// Please Refer GitHub Documentation https://github.com/DFE-Digital/login.dfe.public-api
    /// </para>
    /// </remarks>
    string ServiceAudience { get; }
}
