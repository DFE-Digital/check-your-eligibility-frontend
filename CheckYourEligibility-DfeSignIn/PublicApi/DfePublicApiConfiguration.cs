namespace CheckYourEligibility_DfeSignIn.PublicApi;

public sealed class DfePublicApiConfiguration : IDfePublicApiConfiguration
{
    /// <inheritdoc/>
    public string BaseUrl { get; set; } = null!;

    /// <inheritdoc/>
    public string ApiSecret { get; set; } = null!;

    /// <inheritdoc/>
    public string ClientId { get; set; } = null!;

    /// <inheritdoc/>
    public string ServiceAudience { get; set; } = "signin.education.gov.uk";
}
