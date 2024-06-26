namespace CheckYourEligibility_DfeSignIn.PublicApi;

using CheckYourEligibility_DfeSignIn.Helpers;
using CheckYourEligibility_DfeSignIn.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;

/// <summary>
/// An implementation of <see cref="IDfePublicApi"/> that initiates requests via a
/// <see cref="HttpClient"/>.
/// </summary>
public sealed class DfePublicApi : IDfePublicApi
{
    private readonly HttpClient httpClient;
    private readonly IDfePublicApiConfiguration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="DfePublicApi"/> class.
    /// </summary>
    /// <param name="configuration">Configuration for the DfE Sign-in public API.</param>
    /// <param name="httpClient">Client for making HTTP requests.</param>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="configuration"/> or <paramref name="httpClient"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// If <paramref name="IDfePublicApiConfiguration.BaseUrl"/> is <c>null</c> or empty.
    /// </exception>
    public DfePublicApi(IDfePublicApiConfiguration configuration, HttpClient httpClient)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        if (httpClient == null)
        {
            throw new ArgumentNullException(nameof(httpClient));
        }

        if (string.IsNullOrEmpty(configuration.BaseUrl))
        {
            throw new ArgumentException("Invalid API service URL", nameof(configuration));
        }
        if (string.IsNullOrEmpty(configuration.ApiSecret))
        {
            throw new ArgumentException("Invalid API secret", nameof(configuration));
        }
        if (string.IsNullOrEmpty(configuration.ClientId))
        {
            throw new ArgumentException("Invalid client ID", nameof(configuration));
        }
        if (string.IsNullOrEmpty(configuration.ServiceAudience))
        {
            throw new ArgumentException("Invalid service audience", nameof(configuration));
        }

        this.configuration = configuration;

        this.httpClient = httpClient;
        this.httpClient.BaseAddress = new Uri(configuration.BaseUrl);
        this.httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {CreateBearerToken()}");
    }

    /// <inheritdoc/>
    public async Task<UserAccessToService?> GetUserAccessToService(string userId, string organisationId)
    {
        if (userId == null)
        {
            throw new ArgumentNullException(nameof(userId));
        }
        if (organisationId == null)
        {
            throw new ArgumentNullException(nameof(organisationId));
        }

        var endpoint = $"services/{configuration.ClientId}/organisations/{organisationId}/users/{userId}";
        var response = await httpClient.GetAsync(endpoint);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // User account is not enrolled into service and has no roles.
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new DfePublicApiException(response.StatusCode);
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var userAccessToService = JsonHelpers.Deserialize<UserAccessToService>(responseContent)!;
        return userAccessToService;
    }

    private string CreateBearerToken()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(configuration.ApiSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = configuration.ServiceAudience,
            Issuer = configuration.ClientId,
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
