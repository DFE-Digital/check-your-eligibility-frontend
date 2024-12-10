using CheckYourEligibility_FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Diagnostics;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Server.IIS;

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
            var SchoolList = await _parentService.GetSchool("school");
            if (SchoolList != null)
            {
                SchoolList.Data.ToList();
            }
            else
            {
                return Json(new List<CheckYourEligibility.Domain.Responses.Establishment>());
            }
            return View(SchoolList);
        }
        [HttpPost]
        public  IActionResult SchoolList(string betaschool)
        {
            if (betaschool == "Yes")
            {
                return RedirectToAction("Check/Enter_Details");
            }
            else if (betaschool == "No")
            {
                return RedirectToAction("https://www.gov.uk/apply-free-school-meals"); // this could be in appsettings instead
            }
            else { return View("SchoolList"); //need to also return an error displaying to the user that they must select an option, How to do this? add betaschool to model and create an attribute for it? do it in the controller here? 
        }
    }
}
