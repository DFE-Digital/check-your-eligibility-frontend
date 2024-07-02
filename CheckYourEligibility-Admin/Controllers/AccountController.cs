namespace CheckYourEligibility_FrontEnd.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

public sealed class AccountController : Controller
{
    private readonly ILogger<AccountController> logger;

    public AccountController(
        ILogger<AccountController> logger)
    {
        this.logger = logger;
    }

   
    [HttpGet]
    [Route("/account/sign-out")]
    public IActionResult Logout()
   //    public void Logout()
    {
        if (!(User?.Identity?.IsAuthenticated ?? false))
        {
            return RedirectToAction("Index", "Home");
        }
        //# TODO use cookie name from appsettings.json config
        //        Response.Cookies.Delete("sa-login");

        // return Redirect("https://dev-oidc.signin.education.gov.uk/session/end?clientId=EligibilityCheckingService&post_post_logout_redirect_uri=https://localhost:7728/account/signed-out");
        // return Redirect("https://dev-oidc.signin.education.gov.uk/session/end?clientId=EligibilityCheckingService&post_post_logout_redirect_uri=https://localhost:7728/account/signed-out");


        return SignOut(
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme
        );
        //  SignOut(
        //    CookieAuthenticationDefaults.AuthenticationScheme,
        //    OpenIdConnectDefaults.AuthenticationScheme
        //);
    }

    [HttpGet]
    [Route("/account/signed-out")]
    public IActionResult SignedOut()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction("Index", "Home");
       // return View();
    }
}
