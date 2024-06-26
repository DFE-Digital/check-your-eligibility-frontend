
using System.Security.Claims;
using CheckYourEligibility_DfeSignIn.Constants;
using CheckYourEligibility_DfeSignIn.Helpers;
using CheckYourEligibility_DfeSignIn.Models;

namespace CheckYourEligibility_DfeSignIn.Extensions;
/// <summary>
/// Extension methods for claim related types.
/// </summary>
public static class ClaimExtensions
{
    /// <summary>
    /// Gets the id claim of the user.
    /// </summary>
    /// <returns>
    /// The user id.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="principal"/> is <c>null</c>
    /// </exception>
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        if (principal == null) {
            throw new ArgumentNullException(nameof(principal));
        }

        return principal.Claims
            .Where(c => c.Type.Contains(ClaimConstants.NameIdentifier))
            .Select(c => c.Value)
            .Single();
    }

    /// <summary>
    /// Gets the organisation of a user by deserializing the organisation claim.
    /// </summary>
    /// <returns>
    /// The deserialized <see cref="Organisation"/>, or a value of <c>null</c> if no
    /// organisation has been provided for the user.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="principal"/> is <c>null</c>
    /// </exception>
    public static Organisation? GetOrganisation(this ClaimsPrincipal principal)
    {
        if (principal == null) {
            throw new ArgumentNullException(nameof(principal));
        }

        var organisationJson = principal.Claims.Where(c => c.Type == ClaimConstants.Organisation)
            .Select(c => c.Value)
            .FirstOrDefault();

        if (organisationJson == null) {
            return null;
        }

        var organisation = JsonHelpers.Deserialize<Organisation>(organisationJson)!;

        if (organisation.Id == Guid.Empty) {
            return null;
        }

        return organisation;
    }
}
