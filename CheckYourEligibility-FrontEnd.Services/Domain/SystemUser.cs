using System;

namespace CheckYourEligibility_FrontEnd.Services.Domain;

public class SystemUser
{
    public string? Identifier { get; set; }  // Can store either client_id or username
    public string? Secret { get; set; }      // Can store either client_secret or password
    public string? Scope { get; set; }
    public string? grant_type { get; set; }

    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }

    // Legacy properties for backward compatibility
    public string? Username { get; set; }
    public string? Password { get; set; }
}
