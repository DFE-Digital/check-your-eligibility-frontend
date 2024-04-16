using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility_FrontEnd.ViewModels;
using System.Reflection;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    public class CheckController : Controller
    {
        private readonly ILogger<CheckController> _logger;

        public CheckController(ILogger<CheckController> logger)
        {

            _logger = logger;
        }

        public IActionResult Enter_Details()
        {
            return View();
        }

        public IActionResult Nass()
        {
            return View();
        }

        public IActionResult Loader()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Enter_Details(ParentDetailsViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View("Enter_Details", request);
            }

            var checkEligibilityRequest = new CheckEligibilityRequestDataFsm()
            {
                    LastName = request.LastName,
                    NationalInsuranceNumber = request.NationalInsuranceNumber,
                    DateOfBirth = $"{request.Day}{request.Month}{request.Year}"
            };

            if (request.IsNassSelected == true)
            {
                return View("Nass", checkEligibilityRequest);
            }
            else
            {
                return View("Loader", checkEligibilityRequest);
            }
        } 

    }
}
