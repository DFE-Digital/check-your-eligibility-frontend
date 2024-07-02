using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.Security.Claims;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        DfeClaims? _Claims;

        public HomeController(ILogger<HomeController> logger)
        {
            
            _logger = logger;
            
        }

        public IActionResult Index()
        {
            _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);
            return View(_Claims);
        }

       
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Accessibility()
        {
            return View();
        }

        public IActionResult Cookies()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
