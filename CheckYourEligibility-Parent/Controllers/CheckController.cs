using CheckYourEligibility.Domain.Requests;
using Microsoft.AspNetCore.Mvc;
using CheckYourEligibility_FrontEnd.Services;
using Newtonsoft.Json;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    public class CheckController : Controller
    {
        private readonly ILogger<CheckController> _logger;
        private readonly IEcsServiceParent _service;

        public CheckController(ILogger<CheckController> logger, IEcsServiceParent ecsService)
        {
            _logger = logger;
            _service = ecsService;
        }


        [HttpGet]
        public IActionResult Enter_Details()
        {
            // start with empty page model
            Parent request = null;

            // if this page is loaded again after a POST then get the request and update the page with any errors
            if (TempData["ParentDetails"] != null)
            {
                request = JsonConvert.DeserializeObject<Parent>(TempData["ParentDetails"].ToString());
            }
            if (TempData["Errors"] != null)
            {
                var errors = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(TempData["Errors"].ToString());
                foreach (var kvp in errors)
                {
                    foreach (var error in kvp.Value)
                    {
                        ModelState.AddModelError(kvp.Key, error);
                    }
                }
            }

            return View(request);
        }


        [HttpPost]
        public async Task<IActionResult> Enter_Details(Parent request)
        {
            // dont want to validate nass on this page 
            if (request.IsNassSelected == true)
                ModelState.Remove("NationalAsylumSeekerServiceNumber");

            // do want to validate everything else
            if (!ModelState.IsValid)
            {
                // Use PRG pattern so that after this POST... the page retrieves informaton from tempdata and then goes onto performs a GET to avoid browser resubmit confirm error
                TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList());
                TempData["Errors"] = JsonConvert.SerializeObject(errors);
                return RedirectToAction("Enter_Details");
            }

            // build object for api soft-check
            var checkEligibilityRequest = new CheckEligibilityRequest()
            {
                Data = new CheckEligibilityRequestDataFsm
                {
                    LastName = request.LastName,
                    NationalInsuranceNumber = request.NationalInsuranceNumber?.ToUpper(),
                    DateOfBirth = new DateOnly(request.Year.Value, request.Month.Value, request.Day.Value).ToString("dd/MM/yyyy")
                }
            };

            // set important parent details in session storage
            HttpContext.Session.SetString("ParentFirstName", request.FirstName);
            HttpContext.Session.SetString("ParentLastName", request.LastName);
            HttpContext.Session.SetString("ParentDOB", checkEligibilityRequest.Data.DateOfBirth);

            // if user selected to input nass, save incomplete-model to tempdata and redirect to nass page
            if (request.IsNassSelected == true)
            {
                TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                return RedirectToAction("Nass");
            }

            // otherwise set nino detail in session aswell
            HttpContext.Session.SetString("ParentNINO", request.NationalInsuranceNumber);

            // queue api soft-check
            var response = await _service.PostCheck(checkEligibilityRequest);
            TempData["Response"] = JsonConvert.SerializeObject(response);

            _logger.LogInformation($"Check processed:- {response.Data.Status} {response.Links.Get_EligibilityCheck}");

            // go to loader page which will poll the status
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
            // don't want to validate nino as that has been declared on previous page as not given
            ModelState.Remove("NationalInsuranceNumber");

            // access tempdata and get request
            TempData["Request"] = JsonConvert.SerializeObject(request);

            if (!ModelState.IsValid)
                return View("Nass");

            // if no nass given return couldn't check outcome page
            if (request.NationalAsylumSeekerServiceNumber == null)
                return View("Outcome/Could_Not_Check");
            // otherwise build object and queue soft-check
            else
            {
                // set nass in session storage 
                HttpContext.Session.SetString("ParentNASS", request.NationalAsylumSeekerServiceNumber);

                // build object for api soft-check
                var checkEligibilityRequest = new CheckEligibilityRequest()
                {
                    Data = new CheckYourEligibility.Domain.Requests.CheckEligibilityRequestDataFsm
                    {
                        LastName = request.LastName,
                        NationalAsylumSeekerServiceNumber = request.NationalAsylumSeekerServiceNumber,
                        DateOfBirth = new DateOnly(request.Year.Value, request.Month.Value, request.Day.Value).ToString("dd/MM/yyyy")
                    }
                };

                //TempData["ParentDetails"] = JsonConvert.SerializeObject(request);

                // queue api soft-check
                var response = await _service.PostCheck(checkEligibilityRequest);

                _logger.LogInformation($"Check processed:- {response.Data.Status} {response.Links.Get_EligibilityCheck}");

                // go to loader page which will poll soft-check status
                return RedirectToAction("Loader");
            }
        }

        public IActionResult Loader()
        {
            return View();
        }

        /// this method is called by AJAX
        public async Task<IActionResult> Poll_Status()
        {
            var startTime = DateTime.UtcNow;
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(0.5));

            // gather api response which should either be queuedForProcessing or has a response
            var responseJson = TempData["Response"] as string;
            var response = JsonConvert.DeserializeObject<CheckYourEligibility.Domain.Responses.CheckEligibilityResponse>(responseJson);

            _logger.LogInformation($"Check status processed:- {response.Data.Status} {response.Links.Get_EligibilityCheckStatus}");

            // periodically get status and then render appropriate outcome page
            while (await timer.WaitForNextTickAsync())
            {
                var check = await _service.GetStatus(response);

                if (check.Data.Status != CheckYourEligibility.Domain.Enums.CheckEligibilityStatus.queuedForProcessing.ToString())
                {
                    if (check.Data.Status == CheckYourEligibility.Domain.Enums.CheckEligibilityStatus.eligible.ToString())
                        return View("Outcome/Eligible");

                    if (check.Data.Status == CheckYourEligibility.Domain.Enums.CheckEligibilityStatus.notEligible.ToString())
                        return View("Outcome/Not_Eligible");

                    if (check.Data.Status == CheckYourEligibility.Domain.Enums.CheckEligibilityStatus.parentNotFound.ToString())
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
            // Initialize a new Children object
            var children = new Children();

            // Check if this is a redirect
            if (TempData["IsRedirect"] != null && (bool)TempData["IsRedirect"] == true)
            {
                // Skip validation
                ModelState.Clear();

                // Retrieve updated list from TempData (child could have been added or removed)
                var childListJson = TempData["ChildList"] as string;

                // Transform list to fit model
                children.ChildList = JsonConvert.DeserializeObject<List<Child>>(childListJson);
            }
            else
            {
                // If it's a new page load, populate the ChildList with a new Child
                children.ChildList = new List<Child> { new Child() };
            }

            // Return view and populate with up-to-date child list
            return View(children);
        }


        [HttpPost]
        public IActionResult Enter_Child_Details(Children request)
        {
            if (!ModelState.IsValid)
            {
                return View("Enter_Child_Details", request);
            }

            // create check_answers model, access parent details from session storage and child from pages form
            var fsmApplication = new FsmApplication();
            fsmApplication.ParentFirstName = HttpContext.Session.GetString("ParentFirstName");
            fsmApplication.ParentLastName = HttpContext.Session.GetString("ParentLastName");
            fsmApplication.ParentDateOfBirth = HttpContext.Session.GetString("ParentDOB");
            fsmApplication.ParentNino = HttpContext.Session.GetString("ParentNINO") ?? null;
            fsmApplication.ParentNass = HttpContext.Session.GetString("ParentNASS") ?? null;
            fsmApplication.Children = request;

            return View("Check_Answers", fsmApplication);
        }

        [HttpPost]
        public IActionResult Add_Child(Children request)
        {
            // set initial tempdata
            TempData["IsRedirect"] = true;

            // don't allow the model to contain more than 99 items
            if (request.ChildList.Count > 99)
            {
                return RedirectToAction("Enter_Child_Details");
            }

            request.ChildList.Add(new Child());

            TempData["ChildList"] = JsonConvert.SerializeObject(request.ChildList);

            return RedirectToAction("Enter_Child_Details");
        }


        [HttpPost]
        public IActionResult Remove_Child(Children request, int index)
        {
            // remove child at given index
            var child = request.ChildList[index];
            request.ChildList.Remove(child);

            // set up tempdata so page can be correctly rendered
            TempData["IsRedirect"] = true;
            TempData["ChildList"] = JsonConvert.SerializeObject(request.ChildList);

            return RedirectToAction("Enter_Child_Details");
        }

        /// this method is called by AJAX
        [HttpGet]
        public async Task<IActionResult> GetSchoolDetails(string query)
        {
            // limit api requests to start after 3 chars given
            if (string.IsNullOrEmpty(query) || query.Length < 3)
            {
                return BadRequest("Query must be at least 3 characters long.");
            }

            // make api query
            var results = await _service.GetSchool(query);
            if (results != null)
            {
                // return the results in a list of json
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

        [HttpPost]
        public async Task<IActionResult> Check_Answers(FsmApplication request)
        {
            var responses = new List<ApplicationSaveItemResponse>();

            foreach (var child in request.Children.ChildList)
            {
                var fsmApplication = new ApplicationRequest
                {
                    Data = new ApplicationRequestData()
                    {
                        // Set the properties for each child
                        ParentFirstName = request.ParentFirstName,
                        ParentLastName = request.ParentLastName,
                        ParentDateOfBirth = request.ParentDateOfBirth,
                        ParentNationalInsuranceNumber = request.ParentNino,
                        ParentNationalAsylumSeekerServiceNumber = request.ParentNass,
                        ChildFirstName = child.FirstName,
                        ChildLastName = child.LastName,
                        ChildDateOfBirth = new DateOnly(child.Year.Value, child.Month.Value, child.Day.Value).ToString("dd/MM/yyyy"),
                        School = int.Parse(child.School.URN),
                        UserId = null // get from gov.uk onelogin??
                    }
                };

                // Send each application as an individual check
                var response = await _service.PostApplication(fsmApplication);
                responses.Add(response);
            }

            TempData["FsmApplicationResponses"] = JsonConvert.SerializeObject(responses);
            return RedirectToAction("Application_Sent");
        }

        [HttpGet]
        public IActionResult Application_Sent()
        {
            // Skip validation
            ModelState.Clear();

            return View();
        }
    }
}