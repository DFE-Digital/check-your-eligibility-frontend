using CheckYourEligibility_FrontEnd;
using Azure.Identity;
using CheckYourEligibility_DfeSignIn;
using System.Text;
using CheckYourEligibility_Admin.UseCases;
using CheckYourEligibility_FrontEnd.UseCases.Admin;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddApplicationInsightsTelemetry();
if (Environment.GetEnvironmentVariable("KEY_VAULT_NAME")!=null)
{
    var keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
    var kvUri = $"https://{keyVaultName}.vault.azure.net";

    builder.Configuration.AddAzureKeyVault(new Uri(kvUri), new DefaultAzureCredential());
}

// Add services to the container.
builder.Services.AddServices(builder.Configuration);
builder.Services.AddSession();

builder.Services.AddScoped<IAdminAddChildUseCase, AdminAddChildUseCase>();
builder.Services.AddScoped<IAdminChangeChildDetailsUseCase, AdminChangeChildDetailsUseCase>();
builder.Services.AddScoped<IAdminEnterChildDetailsUseCase, AdminEnterChildDetailsUseCase>();
builder.Services.AddScoped<IAdminLoaderUseCase, AdminLoaderUseCase>();
builder.Services.AddScoped<IAdminLoadParentDetailsUseCase, AdminLoadParentDetailsUseCase>();
builder.Services.AddScoped<IAdminProcessChildDetailsUseCase, AdminProcessChildDetailsUseCase>();
builder.Services.AddScoped<IAdminProcessParentDetailsUseCase, AdminProcessParentDetailsUseCase>();
builder.Services.AddScoped<IAdminRegistrationResponseUseCase, AdminRegistrationResponseUseCase>();
builder.Services.AddScoped<IAdminRegistrationUseCase, AdminRegistrationUseCase>();
builder.Services.AddScoped<IAdminRemoveChildUseCase, AdminRemoveChildUseCase>();
builder.Services.AddScoped<IAdminApplicationsRegisteredUseCase, AdminApplicationsRegisteredUseCase>();
builder.Services.AddScoped<IAdminCreateUserUseCase, AdminCreateUserUseCase>();
builder.Services.AddScoped<IAdminSearchSchoolsUseCase, AdminSearchSchoolsUseCase>();
builder.Services.AddScoped<IAdminSubmitApplicationUseCase, AdminSubmitApplicationUseCase>();
builder.Services.AddScoped<IAdminValidateParentDetailsUseCase, AdminValidateParentDetailsUseCase>();
builder.Services.AddScoped<ISignInUseCase, AdminSignInUseCase>();
builder.Services.AddScoped<IAdminInitializeCheckAnswersUseCase, AdminInitializeCheckAnswersUseCase>();
builder.Services.AddSession();

var dfeSignInConfiguration = new DfeSignInConfiguration();
builder.Configuration.GetSection("DfeSignIn").Bind(dfeSignInConfiguration);
builder.Services.AddDfeSignInAuthentication(dfeSignInConfiguration);

//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
//builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.Use(async (ctx, next) =>
{
    await next();
    if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
    {
        //Re-execute the request so the user gets the error page
        ctx.Request.Path = "/Error/NotFound";
        await next();
    }
});

app.MapHealthChecks("/healthcheck");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
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
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
