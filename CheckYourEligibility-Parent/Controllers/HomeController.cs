using CheckYourEligibility_FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Diagnostics;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Server.IIS;
using CheckYourEligibility_FrontEnd.ViewModels;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    public class HomeController : Controller
    {

        private readonly IEcsServiceParent _parentService;
        public IActionResult Index()
        {
            return View();
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

        public IActionResult fsm_print_version()
        {
            return View();
        }

        public IActionResult Parental_Guidance()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SchoolList()
        {
            var schoolList = await _parentService.GetSchool("school");
            var viewModel = new SchoolListViewModel
            {
                Schools = schoolList?.Data.ToList() ?? new List<CheckYourEligibility.Domain.Responses.Establishment>(),
                IsRadioSelected = true // Default value
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SchoolList(bool? betaschool)
        {
            if (betaschool.HasValue)
            {
                if (betaschool.Value)
                {
                    return RedirectToAction("Check/Enter_Details");
                }
                else
                {
                    return Redirect("https://www.gov.uk/apply-free-school-meals"); // this could be in appsettings instead
                }
            }
            else
            {
                var viewModel = new SchoolListViewModel
                {
                    Schools = new List<CheckYourEligibility.Domain.Responses.Establishment>(), // You might want to fetch the list again
                    IsRadioSelected = false
                };
                return View(viewModel);
            }
        }

    }
}
