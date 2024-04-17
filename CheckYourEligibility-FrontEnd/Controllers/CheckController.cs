using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility_FrontEnd.ViewModels;
using System.Reflection;
using CheckYourEligibility_FrontEnd.Services;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    public class CheckController : Controller
    {
        private readonly ILogger<CheckController> _logger;
        private readonly IEcsService _service;

        public CheckController(ILogger<CheckController> logger, IEcsService ecsService)
        {

            _logger = logger;
            _service = ecsService;
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
        public async Task<IActionResult> Enter_Details(ParentDetailsViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View("Enter_Details", request);
            }
            var checkEligibilityRequest = new CheckEligibilityRequest()
            {
                
                Data = new CheckEligibilityRequestDataFsm
                {
                    LastName = request.LastName,
                    NationalInsuranceNumber = request.NationalInsuranceNumber.ToUpper(),
                    DateOfBirth = new DateOnly(request.Year.Value, request.Month.Value, request.Day.Value).ToString("dd/MM/yyyy")
                }
            };
           var response = await  _service.PostCheck(checkEligibilityRequest);
            _logger.LogInformation($"Check processed:- {response.Data.Status} {response.Links.Get_EligibilityCheck}");

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
