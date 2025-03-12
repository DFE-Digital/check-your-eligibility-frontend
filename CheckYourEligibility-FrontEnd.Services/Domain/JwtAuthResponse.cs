using System;

namespace CheckYourEligibility_FrontEnd.Services.Domain;

public class JwtAuthResponse
{
    public string access_token { get; set; }
    public int expires_in { get; set; }
}
