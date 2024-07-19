using CheckYourEligibility.Domain.Requests;
using Microsoft.AspNetCore.Mvc;
using CheckYourEligibility_FrontEnd.Services;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication;
using GovUk.OneLogin.AspNetCore;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    public class CheckController : Controller
    {
        private readonly ILogger<CheckController> _logger;
        private readonly IEcsServiceParent _service;
        private readonly IConfiguration _config;
        private ILogger<CheckController> _loggerMock;
        private IEcsServiceParent _object;

        public CheckController(ILogger<CheckController> logger, IEcsServiceParent ecsService, IConfiguration configuration)
        {
            _config = configuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = ecsService ?? throw new ArgumentNullException(nameof(ecsService));
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
                // Use PRG pattern
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
                    DateOfBirth = new DateOnly(request.Year.Value, request.Month.Value, request.Day.Value).ToString("yyyy-MM-dd")
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
            TempData["Response"] = JsonConvert.SerializeObject(response, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            _logger.LogInformation($"Check processed:- {response.Data.Status} {response.Links.Get_EligibilityCheck}");

            // go to loader page which will poll the status
            return RedirectToAction("Loader");
        }

        public IActionResult Nass()
        {
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
        public async Task<IActionResult> Nass(Parent request)
        {
            // don't want to validate nino as that has been declared on previous page as not given
            ModelState.Remove("NationalInsuranceNumber");

            // access tempdata and get request
            TempData["Request"] = JsonConvert.SerializeObject(request);

            if (!ModelState.IsValid)
            {
                // Use PRG pattern
                TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList());
                TempData["Errors"] = JsonConvert.SerializeObject(errors);

                return RedirectToAction("Nass");
            }

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
                        DateOfBirth = new DateOnly(request.Year.Value, request.Month.Value, request.Day.Value).ToString("yyyy-MM-dd")
                    }
                };

                TempData["ParentDetails"] = JsonConvert.SerializeObject(request);

                // queue api soft-check
                var response = await _service.PostCheck(checkEligibilityRequest);
                TempData["Response"] = JsonConvert.SerializeObject(response, Formatting.Indented, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

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
                    if (check.Data.Status ==
                        CheckYourEligibility.Domain.Enums.CheckEligibilityStatus.eligible.ToString())
                    {
                        string url = "/check/signIn";
                        return View("Outcome/Eligible", url);
                    }

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

        public IActionResult SignIn()
        {
            var properties = new AuthenticationProperties();
            properties.SetVectorOfTrust(@"[""Cl""]");
            properties.RedirectUri = "/Check/CreateUser";
            return Challenge(properties, authenticationSchemes: OneLoginDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> CreateUser()
        {
            string email = HttpContext.User.Claims.Where(c => c.Type == "email").Select(c => c.Value).First();
            string uniqueId = HttpContext.User.Claims.Where(c => c.Type == "sid").Select(c => c.Value).First();
            
            var user = await _service.CreateUser(
                new UserCreateRequest()
                {
                    Data = new UserData() {
                        Email = email,
                        Reference = uniqueId
                    }
                }
            );
            
            HttpContext.Session.SetString("Email", email);
            HttpContext.Session.SetString("UserId", user.Data);

            return RedirectToAction("Enter_Child_Details");
        }

        public IActionResult Enter_Child_Details()
        {
            var children = new Children() { ChildList = [new()] };

            // Check if this is a redirect after add or remove child
            if (TempData["IsChildAddOrRemove"] != null && (bool)TempData["IsChildAddOrRemove"] == true)
            {
                ModelState.Clear();

                // Retrieve Children from TempData
                var childDetails = TempData["ChildList"] as string;
                children.ChildList = JsonConvert.DeserializeObject<List<Child>>(childDetails);
            }

            return View(children);
        }


        [HttpPost]
        public IActionResult Enter_Child_Details(Children request)
        {
            if (TempData["FsmApplication"] != null && TempData["IsRedirect"] != null && (bool)TempData["IsRedirect"] == true)
            {
                return View(request);
            }

            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var fsmApplication = new FsmApplication
            {
                ParentFirstName = HttpContext.Session.GetString("ParentFirstName"),
                ParentLastName = HttpContext.Session.GetString("ParentLastName"),
                ParentDateOfBirth = HttpContext.Session.GetString("ParentDOB"),
                ParentNino = HttpContext.Session.GetString("ParentNINO") ?? null,
                ParentNass = HttpContext.Session.GetString("ParentNASS") ?? null,
                Children = request,
                Email = HttpContext.Session.GetString("Email")
            };

            TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);

            return View("Check_Answers", fsmApplication);
        }

        [HttpPost]
        public IActionResult Add_Child(Children request)
        {
            // set initial tempdata
            TempData["IsChildAddOrRemove"] = true;

            // don't allow the model to contain more than 99 items
            if (request.ChildList.Count >= 99)
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
            TempData["IsChildAddOrRemove"] = true;
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
                        ChildDateOfBirth = new DateOnly(child.Year.Value, child.Month.Value, child.Day.Value).ToString("yyyy-MM-dd"),
                        School = int.Parse(child.School.URN),
                        UserId = HttpContext.Session.GetString("UserId"),
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

        public IActionResult ChangeChildDetails()
        {
            // set up tempdata and access existing temp data object
            TempData["IsRedirect"] = true;
            var responseJson = TempData["FsmApplication"] as string;
            // deserialize
            var responses = JsonConvert.DeserializeObject<FsmApplication>(responseJson);
            // get children details
            var children = responses.Children;
            // populate enter_child_details page with children model
            return View("Enter_Child_Details", children);
        }
    }
}