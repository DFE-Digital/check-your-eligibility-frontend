using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd;
using Azure.Identity;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Microsoft.ApplicationInsights.Extensibility;
using CheckYourEligibility_FrontEnd.Telemetry;
using Microsoft.AspNetCore.Http;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry();

// Register services from ProgramExtensions
builder.Services.AddServices(builder.Configuration);

// Add session support
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// Load configuration from Azure Key Vault if available
if (Environment.GetEnvironmentVariable("KEY_VAULT_NAME") != null)
{
    var keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
    var kvUri = $"https://{keyVaultName}.vault.azure.net";

    builder.Configuration.AddAzureKeyVault(new Uri(kvUri), new DefaultAzureCredential());
}

// Configure authentication
builder.Services.AddAuthentication(defaultScheme: OneLoginDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOneLogin(options =>
    {
        // Specify the authentication scheme to persist user information with
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        // Configure the endpoints for the One Login environment you're targeting
        options.MetadataAddress = builder.Configuration["OneLogin:Host"] + "/.well-known/openid-configuration";
        options.ClientAssertionJwtAudience = builder.Configuration["OneLogin:Host"] + "/token";

        // Configure client information
        options.ClientId = builder.Configuration["OneLogin:ClientId"];
        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/onelogin-logout-callback";
        options.CoreIdentityClaimIssuer = builder.Configuration["OneLogin:Host"].Replace("oidc", "identity");

        // Configure the private key used for authentication
        string privateKey = builder.Configuration["OneLogin:PrivateKey"];
        using (var rsa = RSA.Create())
        {
            rsa.ImportFromPem(privateKey.ToCharArray());
            Console.WriteLine("successful");
            options.ClientAuthenticationCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: true)), SecurityAlgorithms.RsaSha256);
        }

        // Configure vectors of trust
        options.VectorOfTrust = @"[""Cl""]";

        // Override the cookie name prefixes (optional)
        options.CorrelationCookie.Name = "check-your-eligibility-onelogin-correlation.";
        options.NonceCookie.Name = "check-your-eligibility-onelogin-nonce.";
    });

var app = builder.Build();

// Add middleware to enable response body buffering
app.Use(async (context, next) =>
{
    // Keep a reference to the original response body stream
    var originalBodyStream = context.Response.Body;

    // Create a new memory stream to hold the response
    using (var responseBody = new MemoryStream())
    {
        // Replace the response body stream with our memory stream
        context.Response.Body = responseBody;

        // Call the next middleware in the pipeline
        await next();

        // Reset the memory stream position to the beginning
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        // Read the response body (optional: you can remove this if not needed here)
        // string responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();

        // Reset the memory stream position again after reading
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        // Copy the contents of the new memory stream to the original stream
        await responseBody.CopyToAsync(originalBodyStream);
    }
});

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. Consider changing this for production scenarios
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
