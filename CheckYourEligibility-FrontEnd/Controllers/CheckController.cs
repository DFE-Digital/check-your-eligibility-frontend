using Microsoft.AspNetCore.Mvc;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility_FrontEnd.ViewModels;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.Domain.Enums;
using Newtonsoft.Json;

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

        public IActionResult Loader()
        {
            return View();
        }

        public IActionResult Enter_Details()
        {
            return View();
        }

        public IActionResult Nass()
        {
            var parentDetailsViewModel = new ParentDetailsViewModel();

            return View(parentDetailsViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Nass(ParentDetailsViewModel request)
        {
            ModelState.Remove("NationalInsuranceNumber");

            TempData["Request"] = JsonConvert.SerializeObject(request);

            if (!ModelState.IsValid)
            {
                return View("Nass");
            }

            if (request.NationalAsylumSeekerServiceNumber == null)
            {
                return View("Outcome/Could_Not_Check");
            }
            else
            {
                var checkEligibilityRequest = new CheckEligibilityRequest()
                {
                    Data = new CheckEligibilityRequestDataFsm
                    {
                        LastName = request.LastName,
                        NationalAsylumSeekerServiceNumber = request.NationalAsylumSeekerServiceNumber,
                        DateOfBirth = new DateOnly(request.Year.Value, request.Month.Value, request.Day.Value).ToString("dd/MM/yyyy")
                    }
                };

                var response = await _service.PostCheck(checkEligibilityRequest);

                _logger.LogInformation($"Check processed:- {response.Data.Status} {response.Links.Get_EligibilityCheck}");

                return RedirectToAction("Loader");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Enter_Details(ParentDetailsViewModel request)
        {
            if (request.IsNassSelected == true)
            {
                ModelState.Remove("NationalAsylumSeekerServiceNumber");
            }

            if (!ModelState.IsValid)
            {
                return View("Enter_Details", request);
            }
            var checkEligibilityRequest = new CheckEligibilityRequest()
            {
                Data = new CheckEligibilityRequestDataFsm
                {
                    LastName = request.LastName,
                    NationalInsuranceNumber = request.NationalInsuranceNumber?.ToUpper(),
                    DateOfBirth = new DateOnly(request.Year.Value, request.Month.Value, request.Day.Value).ToString("dd/MM/yyyy")
                }
            };

            if (request.IsNassSelected == true)
            {
                TempData["Request"] = JsonConvert.SerializeObject(request);

                return RedirectToAction("Nass");
            }

            var response = await _service.PostCheck(checkEligibilityRequest);

            TempData["Response"] = JsonConvert.SerializeObject(response);

            _logger.LogInformation($"Check processed:- {response.Data.Status} {response.Links.Get_EligibilityCheck}");

            return RedirectToAction("Loader");
        }

        public async Task<IActionResult> Poll_Status()
        {
            var startTime = DateTime.UtcNow;
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(0.5));

            var responseJson = TempData["Response"] as string;
            var response = JsonConvert.DeserializeObject<CheckEligibilityResponse>(responseJson);

            _logger.LogInformation($"Check status processed:- {response.Data.Status} {response.Links.Get_EligibilityCheckStatus}");

            while (await timer.WaitForNextTickAsync())
            {
                var check = await _service.GetStatus(response);

                if (check.Data.Status != CheckEligibilityStatus.queuedForProcessing.ToString())
                {
                    if (check.Data.Status == CheckEligibilityStatus.eligible.ToString())
                        return View("Outcome/Eligible");

                    if (check.Data.Status == CheckEligibilityStatus.notEligible.ToString())
                        return View("Outcome/Not_Eligible");

                    if (check.Data.Status == CheckEligibilityStatus.parentNotFound.ToString())
                        return View("Outcome/Not_Found");

                    break;
                }
                else
                {
                    if ((DateTime.UtcNow - startTime).TotalMinutes > 2)
                    {
                        break;
                    }
                    continue;
                }
            }
            return View("Outcome/Default");
        }
    }
}
