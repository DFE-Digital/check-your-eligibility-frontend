using Microsoft.AspNetCore.Mvc;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.Domain.Enums;
using Newtonsoft.Json;
using CheckYourEligibility_FrontEnd.Models;

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

        [HttpPost]
        public async Task<IActionResult> Enter_Details(Parent request)
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

        public IActionResult Nass()
        {
            var parent = new Parent();

            return View(parent);
        }

        [HttpPost]
        public async Task<IActionResult> Nass(Parent request)
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

        public IActionResult Loader()
        {
            return View();
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

        public IActionResult Enter_Child_Details()
        {
            // Check if this is a redirect
            if (TempData["IsRedirect"] != null && (bool)TempData["IsRedirect"])
            {
                // Clear the model state to skip validation
                ModelState.Clear();

                // Retrieve the updated list from TempData
                var childListJson = TempData["ChildList"] as string;
                var childList = JsonConvert.DeserializeObject<List<Child>>(childListJson);

                var children = new Children { ChildList = childList };

                return View(children);
            }
            else
            {
                var children = new Children()
                {
                    ChildList = [new Child()]
                };

                return View(children);
            }
        }

        [HttpPost]
        public IActionResult Enter_Child_Details(Children request)
        {
            if (!ModelState.IsValid)
            {
                return View("Enter_Child_Details", request);
            }

            for (int i = 0; i <= request.ChildList.Count - 1; i++)
            {
                Console.WriteLine(request.ChildList[i].FirstName);
                Console.WriteLine(request.ChildList[i].LastName);
                Console.WriteLine(request.ChildList[i].Day);
                Console.WriteLine(request.ChildList[i].Month);
                Console.WriteLine(request.ChildList[i].Year);
                Console.WriteLine(request.ChildList[i].School.Name);
                Console.WriteLine(request.ChildList[i].School.LA);
                Console.WriteLine(request.ChildList[i].School.Postcode);
                Console.WriteLine(request.ChildList[i].School.URN);
            }

            // request stores children data, parent retrieved and combined to make application
            Parent parent = null;

            FsmApplication fsmApplication = new FsmApplication(parent, request);

            return View(request);
        }

        public IActionResult Add_Child(Children request)
        {
            TempData["IsRedirect"] = true;

            if (request.ChildList.Count > 99)
            {
                return RedirectToAction("Enter_Child_Details", request);
            }

            request.ChildList.Add(new Child());

            TempData["ChildList"] = JsonConvert.SerializeObject(request.ChildList);

            return RedirectToAction("Enter_Child_Details", request);
        }

        [HttpPost]
        public IActionResult Remove_Child(Children request, int index)
        {
            var child = request.ChildList[index];
            request.ChildList.Remove(child);

            TempData["IsRedirect"] = true;
            TempData["ChildList"] = JsonConvert.SerializeObject(request.ChildList);

            return RedirectToAction("Enter_Child_Details");
        }

        [HttpGet]
        public async Task<IActionResult> GetSchoolDetails(string query)
        {
            if (string.IsNullOrEmpty(query) || query.Length < 3)
            {
                return BadRequest("Query must be at least 3 characters long.");
            }

            var results = await _service.GetSchool(query);
            if (results != null)
            {
                return Json(results.Data.ToList());
            }
            else
            {
                return null;
            }
        }

        public IActionResult Check_Answers()
        {
            return View();
        }


        public IActionResult Application_Sent()
        {
            return View();
        }
    }
}