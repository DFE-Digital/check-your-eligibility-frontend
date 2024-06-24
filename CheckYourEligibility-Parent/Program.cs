using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd;
using Azure.Identity;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

if (Environment.GetEnvironmentVariable("KEY_VAULT_NAME")!=null)
{
    var keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
    var kvUri = $"https://{keyVaultName}.vault.azure.net";

    builder.Configuration.AddAzureKeyVault(new Uri(kvUri), new DefaultAzureCredential());
}

// Add services to the container.
builder.Services.AddServices(builder.Configuration);

builder.Services.AddAuthentication(defaultScheme: OneLoginDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOneLogin(options =>
    {
        // Specify the authentication scheme to persist user information with.
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        // Configure the endpoints for the One Login environment you're targeting.
        options.MetadataAddress = "https://oidc.integration.account.gov.uk/.well-known/openid-configuration";
        options.ClientAssertionJwtAudience = "https://oidc.integration.account.gov.uk/token";

        // Configure your client information.
        // CallbackPath and SignedOutCallbackPath must align with the redirect_uris and post_logout_redirect_uris configured in One Login.
        options.ClientId = builder.Configuration["OneLogin:ClientId"];
        options.CallbackPath = builder.Configuration["Host"] + "/Check/Enter_Child_Details";
        options.SignedOutCallbackPath = "/onelogin-logout-callback";

        // Configure the private key used for authentication.
        // See the RSA class' documentation for the various ways to do this.
        // Here we're loading a PEM-encoded private key from configuration.
        string privateKey = @"-----BEGIN PRIVATE KEY-----
MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCrqo1eayUuowCg
wddbbqDXZr/BcUREQUxcfqFzAC9ZoJRTOOKl4/8zVPBmZGoITVROSrPTi/XCsdK2
EGb0leRUM8Sg7ClmqrMvDO51kALlHHHQ965W2ETra7zvfWcLnVBEN+EnEBjLoJ1T
b+YvRPoGrB5KvbEd26sD1n2jvK6gFEYPV+JqlMldD/XCz8VNozHaNLjN+dN0IZMN
UbAOa1dE3h+8fMPVjR/sS/m8673C+e/r+qIn3brEV05JZBcsC4DIyyoaps/obY2c
Q62VsDZIn47gQS+Emg/iOc/M9YRXoVnhYFaiPsO0LpIt5cCNPxiEnC4w7zf+O3aU
LAat79TRAgMBAAECggEAB794qvY2CcObfW8bbgDM4ldhOGLSRZoW4ZPW/lMhBtSi
f3WoLI4qUE+5Lj3msvSDHGClw9qag/FHsrhfgu4Z1uPssBL2WWb57z5ZYuf6s8GK
jBwpspS+I5UnDWKux9/GbwOApBLr/LeqBG36SOJQdSkQ2cwF1mI59vcFtM/SOigI
iLfO4J5GCPmrHzzZY+2kMx8A7H0dnqBP3i7oO1cpq4PGhMcTf21iuE0s7ME9GNrd
FjxevzrGVTcc4VYLl+LLnCbeSU01oz/+zljz01xJhvAd5wV/H9pf4rb6w327jIFz
dpzliuQSMBNYUZK/JuQucYMPPa93cohU4n0mnwfsgwKBgQDjvclfyKfBPsjZe0H/
/S3vvFmqqOuGClUPYQaI/7tJsn7iv2Rf40UvR9y5mSQBd/u8CT42ydbpD3ScBo3k
BMJePumgyv8AfKEfwreQtecK2ii1UOIZsfkvMM760Nud4lDqQ9P5D7XAe1tq+4wk
meGVcH/JpBJIQuEyEWV+gVP1SwKBgQDA94ljuq8QKHEaIuW9rik4xxlyE4uTeIFk
WxOFxJNRiG4Vl8C5ru7KPIAoNy1l2Vwveb1XjT+gLxnRi8vQaMMRVSnSJPN55cZt
gtLGnYSiVXblAma1EetMxSyQMEbp4Ydojmoo6IpworsYk8pJTHLwx1ypuTG6jHtV
9AgYGtT40wKBgCQpMF5bF/fhJjcSESq6Yp7cQ0iLxcnkvhjRCR6brHtJMkiCp0dq
aMPXHz8BB+yLxpbWyOAeMFeVMqjLiaAY+VvJlYMIeHD1WQgX/NmnaGYaubgAfcYi
sjRCBbthil2JX9uypWe4jN1hOOTFyDzPijgWQtQbjyOWKveuN3Vcx539AoGAGLrg
ma5gJzL3o8DbLp72W3dwtGT621BzTLg7XUZfFvDkItJK+cEIi1SLnBvOLqJXpSH1
+RV6FP5UUb2XxkLW1Q7UCEGCDpo6/ufoOVoQmHlZfmn7XbNJM5KFbokxXWHw555w
zoNW0q6YScMPBqvMgz0ZjArW67B7Uf2vV9+Acc8CgYBudCDUldmXiSZnw+FeSHkz
W2Qy/y+VXodj4b1bUT/QES1dQ/viwSPw0dJnkG9+Z/ubYDrYQ6c43RFVQ53vTto8
FsbMA2wad55c7skSf2gZj3KNC2Oa3wsY7WlGIdr8uThX55JD1D3SJjVnsPLyF8cH
JqzQwbBadmJaIUqtTYTcFA==
-----END PRIVATE KEY-----
";
        using (var rsa = RSA.Create())
        {
            rsa.ImportFromPem(privateKey.ToCharArray());
            Console.WriteLine("successful");


            options.ClientAuthenticationCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: true)), SecurityAlgorithms.RsaSha256);
        }

        // Configure vectors of trust.
        // See the One Login docs for the various options to use here.
        options.VectorOfTrust = @"[""Cl""]";

        // Override the cookie name prefixes (optional)
        options.CorrelationCookie.Name = "my-app-onelogin-correlation.";
        options.NonceCookie.Name = "my-app-onelogin-nonce.";
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
