using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd;
using Azure.Identity;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();
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
        //specify the authentication scheme to persist user information with
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        //configue the endpoints for the One Login environment you're targeting
        options.MetadataAddress = builder.Configuration["OneLogin:Host"]+"/.well-known/openid-configuration";
        options.ClientAssertionJwtAudience = builder.Configuration["OneLogin:Host"]+"/token";

        //configure client information
        // CallbackPath and SignedOutCallbackPath must align with the redirect_uris and post_logout_redirect_uris configured in One Login.

        options.ClientId = builder.Configuration["OneLogin:ClientId"];
        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/onelogin-logout-callback";
        options.CoreIdentityClaimIssuer = builder.Configuration["OneLogin:Host"].Replace("oidc", "identity");

        // Configure the private key used for authentication.
        // See the RSA class' documentation for the various ways to do this.
        // Here we're loading a PEM-encoded private key from configuration.

        string privateKey = builder.Configuration["OneLogin:PrivateKey"];
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
        options.CorrelationCookie.Name = "check-your-eligibility-onelogin-correlation.";
        options.NonceCookie.Name = "check-your-eligibility-onelogin-nonce.";
    });

//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
//builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/healthcheck");

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

app.Use(async (httpContext, next) =>
{
    try
    {
        httpContext.Request.EnableBuffering();
        string requestBody = await new StreamReader(httpContext.Request.Body, Encoding.UTF8).ReadToEndAsync();
        httpContext.Request.Body.Position = 0;
        app.Logger.LogInformation($"Request body: {requestBody}");
    }
    catch (Exception ex)
    {
        app.Logger.LogInformation($"Exception reading request: {ex.Message}");
    }

    Stream originalBody = httpContext.Response.Body;
    try
    {
        using var memStream = new MemoryStream();
        httpContext.Response.Body = memStream;

        // call to the following middleware 
        // response should be produced by one of the following middlewares
        await next(httpContext);

        memStream.Position = 0;
        string responseBody = new StreamReader(memStream).ReadToEnd();

        memStream.Position = 0;
        await memStream.CopyToAsync(originalBody);
        app.Logger.LogInformation($"Response body: {responseBody}");
    }
    finally
    {
        httpContext.Response.Body = originalBody;
    }
});

app.Run();
