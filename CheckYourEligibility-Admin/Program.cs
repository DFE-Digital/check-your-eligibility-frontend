using CheckYourEligibility_FrontEnd;
using Azure.Identity;
using CheckYourEligibility_DfeSignIn;
using System.Text;

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

var dfeSignInConfiguration = new DfeSignInConfiguration();
builder.Configuration.GetSection("DfeSignIn").Bind(dfeSignInConfiguration);
builder.Services.AddDfeSignInAuthentication(dfeSignInConfiguration);

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

app.UseRouting();
app.UseAuthorization();
app.UseSession();
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
