using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd;
using Azure.Identity;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// ------------------------
// 1. Configure Services
// ------------------------

// Application Insights Telemetry
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    // Disable adaptive sampling to capture all telemetry
    options.EnableAdaptiveSampling = false;
});

// Add Application Insights as a Logging Provider
builder.Logging.AddApplicationInsights();

// Add Session
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

// Add services to the container.
builder.Services.AddServices(builder.Configuration);

// Configure OneLogin Authentication
builder.Services.AddAuthentication(defaultScheme: OneLoginDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOneLogin(options =>
    {
        // Authentication configuration...
        // [Your existing OneLogin configuration code]
    });

// Register IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Controllers with Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ------------------------
// 2. Configure Middleware Pipeline
// ------------------------

// 2.1. Exception Handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// 2.2. HTTPS Redirection and Static Files
app.UseHttpsRedirection();
app.UseStaticFiles();

// 2.3. Routing
app.UseRouting();

// 2.4. Session Middleware
app.UseSession();

// 2.5. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 2.6. Set AuthenticatedUserId for Application Insights
app.Use(async (context, next) =>
{
    if (context.User.Identity.IsAuthenticated)
    {
        var userId = context.User.Identity.Name;

        // Optional: Hash or anonymize the userId for privacy compliance
        // For example:
        // userId = HashUserId(userId);

        var requestTelemetry = context.Features.Get<RequestTelemetry>();
        if (requestTelemetry != null)
        {
            requestTelemetry.Context.User.AuthenticatedUserId = userId;
        }
    }

    await next.Invoke();
});

// 2.7. Map Controller Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 2.8. Run the App
app.Run();
