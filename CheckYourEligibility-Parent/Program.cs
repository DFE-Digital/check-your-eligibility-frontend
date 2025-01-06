using CheckYourEligibility_FrontEnd;
using Azure.Identity;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using CheckYourEligibility_FrontEnd.UseCases.Schools;
using CheckYourEligibility_FrontEnd.UseCases.Schools.GetSchoolDetailsUseCase;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

if (Environment.GetEnvironmentVariable("KEY_VAULT_NAME") != null)
{
    var keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
    var kvUri = $"https://{keyVaultName}.vault.azure.net";
    builder.Configuration.AddAzureKeyVault(new Uri(kvUri), new DefaultAzureCredential());
}

// Add services to the container
builder.Services.AddServices(builder.Configuration);
builder.Services.AddScoped<IGetSchoolDetailsUseCase, GetSchoolDetailsUseCase>();


builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme = "UNKNOWN";
    opt.DefaultChallengeScheme = "UNKNOWN";
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOneLogin(options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.MetadataAddress = builder.Configuration["OneLogin:Host"] + "/.well-known/openid-configuration";
    options.ClientAssertionJwtAudience = builder.Configuration["OneLogin:Host"] + "/token";
    options.ClientId = builder.Configuration["OneLogin:ClientId"];
    options.CallbackPath = "/signin-oidc";
    options.SignedOutCallbackPath = "/onelogin-logout-callback";
    options.CoreIdentityClaimIssuer = builder.Configuration["OneLogin:Host"].Replace("oidc", "identity");

    string privateKey = builder.Configuration["OneLogin:PrivateKey"];
    using (var rsa = RSA.Create())
    {
        rsa.ImportFromPem(privateKey.ToCharArray());
        Console.WriteLine("successful");
        options.ClientAuthenticationCredentials = new SigningCredentials(
            new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: true)), SecurityAlgorithms.RsaSha256);
    }
    options.VectorOfTrust = @"[""Cl""]";
    options.CorrelationCookie.Name = "check-your-eligibility-onelogin-correlation.";
    options.NonceCookie.Name = "check-your-eligibility-onelogin-nonce.";
}).AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>
    ("BasicAuthentication", null).AddPolicyScheme("UNKNOWN", "UNKNOWN", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            if (context.Request.Path == "/Check/CreateUser")
            {
                return "OneLogin";
            }
            Console.WriteLine(context.Request.Path);
            return "BasicAuthentication";
        };
    });

builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.Use(async (ctx, next) =>
{
    await next();
    if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
    {
        ctx.Request.Path = "/Error/NotFound";
        await next();
    }
});

app.MapHealthChecks("/healthcheck");

app.Use((context, next) =>
{
    context.Response.Headers["strict-transport-security"] = "max-age=31536000; includeSubDomains";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self' https://*.clarity.ms https://c.bing.com";
    context.Response.Headers["X-Frame-Options"] = "sameorigin";
    context.Response.Headers["Cache-Control"] = "Private";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    return next.Invoke();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();