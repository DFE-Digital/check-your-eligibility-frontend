namespace CheckYourEligibility_DfeSignIn.PublicApi;

using System.Net;

/// <summary>
/// An exception which is thrown when a problem occurs whilst interacting with the
/// DfE Sign-in public API.
/// </summary>
public sealed class DfePublicApiException : Exception
{
    /// <summary>
    /// Initialises a new instance of the <see cref="DfePublicApiException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    public DfePublicApiException(HttpStatusCode statusCode)
        : base($"API responded with status code {statusCode}")
    {
    }
}
