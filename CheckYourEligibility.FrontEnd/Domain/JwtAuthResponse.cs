using System;

namespace CheckYourEligibility.FrontEnd.Domain;

public class JwtAuthResponse
{
    public string access_token { get; set; }
    public int expires_in { get; set; }
}
