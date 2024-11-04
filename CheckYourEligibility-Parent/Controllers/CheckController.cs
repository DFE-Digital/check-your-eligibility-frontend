using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Child = CheckYourEligibility_FrontEnd.Models.Child;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    public class CheckController : Controller
    {
        private readonly ILogger<CheckController> _logger;
        private readonly IEcsServiceParent _parentService;
        private readonly IEcsCheckService _checkService;
        private readonly IConfiguration _config;
        private ILogger<CheckController> _loggerMock;
        private IEcsServiceParent _object;

        public CheckController(ILogger<CheckController> logger, IEcsServiceParent ecsParentService, IEcsCheckService ecsCheckService, IConfiguration configuration)
        {
            _config = configuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentService = ecsParentService ?? throw new ArgumentNullException(nameof(ecsParentService));
            _checkService = ecsCheckService ?? throw new ArgumentNullException(nameof(ecsCheckService));

            _logger.LogInformation("controller log info");
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
            if (request.IsNinoSelected == false || request.IsNinoSelected == null)
            {
                ModelState.Remove("NationalInsuranceNumber");
                if (!request.NASSRedirect)
                {
                    ModelState.Remove("NationalAsylumSeekerServiceNumber");
                }

            }
            if (!request.IsNinoSelected == null)
            {
                ModelState.Remove("IsNassSelected"); 
            }
            else
            {
                request.NASSRedirect = false;
            }

            // do want to validate everything else
            if (!ModelState.IsValid)
            {
                // Use PRG pattern
                // POST (this method)
                // RETRIEVE and store informaton from tempdata then
                // GET inital page where errors are rendered
                TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList());
                TempData["Errors"] = JsonConvert.SerializeObject(errors);
                if (request.NASSRedirect && request.IsNinoSelected == false)
                {
                    return RedirectToAction("Nass");
                }
                else if ((errors.ContainsKey("IsNinoSelected") && request.NASSRedirect == true) || errors.ContainsKey("NationalAsylumSeekerServiceNumber"))
                {
                    return View("Nass");
                }
                return RedirectToAction("Enter_Details");
            }

            if (request.IsNassSelected == false)
            {
                TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                ModelState.Clear();
                return View("Outcome/Could_Not_Check");
            }

            // build object for api soft-check
            var checkEligibilityRequest = new CheckEligibilityRequest_Fsm()
            {
                Data = new CheckEligibilityRequestData_Fsm
                {
                    LastName = request.LastName,
                    NationalInsuranceNumber = request.NationalInsuranceNumber?.ToUpper(),
                    NationalAsylumSeekerServiceNumber = request.NationalAsylumSeekerServiceNumber?.ToUpper(),
                    DateOfBirth = new DateOnly(int.Parse(request.Year), int.Parse(request.Month), int.Parse(request.Day)).ToString("yyyy-MM-dd")
                }
            };

            // set important parent details in session storage
            HttpContext.Session.SetString("ParentFirstName", request.FirstName);
            HttpContext.Session.SetString("ParentLastName", request.LastName);
            HttpContext.Session.SetString("ParentDOB", checkEligibilityRequest.Data.DateOfBirth);

            // if user selected to input nass, save incomplete-model to tempdata and redirect to nass page
            if (request.IsNinoSelected == false && !request.NASSRedirect)
            {
                request.NASSRedirect = true;
                TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                return RedirectToAction("Nass");
            }

            // otherwise set nino and NASS detail in session
            if (!string.IsNullOrEmpty(request.NationalInsuranceNumber))
            {
                HttpContext.Session.SetString("ParentNINO", request.NationalInsuranceNumber);
            }
            if (!string.IsNullOrEmpty(request.NationalAsylumSeekerServiceNumber))
            {
                HttpContext.Session.SetString("ParentNASS", request.NationalAsylumSeekerServiceNumber);
            }

            // queue api soft-check
            var response = await _checkService.PostCheck(checkEligibilityRequest);
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
            var parentDetails = TempData["ParentDetails"];
            if (parentDetails == null)
            {
                return RedirectToAction("Enter_Details");
            }
            var parent = new Parent();

            return View(parent);
        }


        public async Task<IActionResult> Loader()
        {
            // Retrieve the API response from TempData
            var responseJson = TempData["Response"] as string;
            if (responseJson == null)
            {
                _logger.LogWarning("No response data found in TempData.");
                return View("Outcome/Technical_Error");
            }

            var response = JsonConvert.DeserializeObject<CheckEligibilityResponse>(responseJson);

            _logger.LogInformation($"Check status processed: {response.Data.Status}");

            // Call the service to check the current status
            var check = await _checkService.GetStatus(response);

            if (check == null || check.Data == null)
            {
                _logger.LogWarning("Null response received from GetStatus.");
                return View("Outcome/Technical_Error");
            }

            _logger.LogInformation($"Received status: {check.Data.Status}");

            SetSessionCheckResult(check.Data.Status);

            // Handle final statuses and redirect appropriately
            switch (check.Data.Status)
            {
                case "eligible":
                    return View("Outcome/Eligible", "/check/signIn");

                case "notEligible":
                    return View("Outcome/Not_Eligible");

                case "parentNotFound":
                    return View("Outcome/Not_Found");

                case "DwpError":
                    return View("Outcome/Technical_Error");

                case "queuedForProcessing":
                    _logger.LogInformation("Still queued for processing.");
                    // Save the response back to TempData for the next poll
                    TempData["Response"] = JsonConvert.SerializeObject(response);
                    // Render the loader view which will auto-refresh
                    return View("Loader");

                default:
                    _logger.LogError("Unexpected status received.");
                    return View("Outcome/Technical_Error");
            }
        }

        public void SetSessionCheckResult(string status)
        {
            HttpContext.Session.SetString("CheckResult", status);
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
            string uniqueId = HttpContext.User.Claims.Where(c => c.Type == "sub").Select(c => c.Value).First();

            var user = await _parentService.CreateUser(
                new UserCreateRequest()
                {
                    Data = new UserData()
                    {
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
        public async Task<IActionResult> Enter_Child_Details(Children request)
        {
            if (TempData["FsmApplication"] != null && TempData["IsRedirect"] != null && (bool)TempData["IsRedirect"] == true)
            {
                return View(request);
            }

            var idx = 0;
            foreach (var item in request.ChildList)
            {
                if (item.School.URN == null) continue;
                if (item.School.URN.Length == 6 && int.TryParse(item.School.URN, out _))
                {
                    var schools = await _parentService.GetSchool(item.School.URN);

                    if (schools!=null)
                    {
                        item.School.Name = schools.Data.First().Name;
                        ModelState.Remove($"ChildList[{idx}].School.URN");
                    }

                    else
                    {
                        ModelState.AddModelError($"ChildList[{idx}].School.URN", "The selected school does not exist in our service.");
                    }
                }
                else
                {
                    ModelState.AddModelError($"ChildList[{idx}].School.URN", "School URN should be a 6 digit number.");
                }
                idx++;
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
            var results = await _parentService.GetSchool(query);
            if (results != null)
            {
                // return the results in a list of json
                return Json(results.Data.ToList());
            }
            else
            {
                return Json(new List<CheckYourEligibility.Domain.Responses.School>());
            }
        }

        public IActionResult Check_Answers()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Check_Answers(FsmApplication request)
        {
            var currentStatus = HttpContext.Session.GetString("CheckResult");
            _logger.LogInformation($"Current eligibility status in session: {currentStatus}");

            if (currentStatus != CheckYourEligibility.Domain.Enums.CheckEligibilityStatus.eligible.ToString())
            {
                throw new Exception($"Invalid status when trying to create an application: {currentStatus}");
            }
            List<ApplicationSaveItemResponse> responses = new List<ApplicationSaveItemResponse>();
            
            foreach (var child in request.Children.ChildList)
            {
                var fsmApplication = new ApplicationRequest
                {
                    Data = new ApplicationRequestData()
                    {
                        Type = CheckEligibilityType.FreeSchoolMeals,
                        // Set the properties for each child
                        ParentFirstName = request.ParentFirstName,
                        ParentLastName = request.ParentLastName,
                        ParentDateOfBirth = request.ParentDateOfBirth,
                        ParentNationalInsuranceNumber = request.ParentNino,
                        ParentNationalAsylumSeekerServiceNumber = request.ParentNass,
                        ChildFirstName = child.FirstName,
                        ChildLastName = child.LastName,
                        ChildDateOfBirth = new DateOnly(int.Parse(child.Year), int.Parse(child.Month), int.Parse(child.Day)).ToString("yyyy-MM-dd"),
                        School = int.Parse(child.School.URN),
                        UserId = HttpContext.Session.GetString("UserId"),
                        ParentEmail = HttpContext.Session.GetString("Email"),
                    }
                };
                
                // Send each application as an individual check
                var response = await _parentService.PostApplication_Fsm(fsmApplication);
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
