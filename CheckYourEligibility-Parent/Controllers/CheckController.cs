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
using CheckYourEligibility_FrontEnd.UseCases;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    public class CheckController : Controller
    {
        private readonly ILogger<CheckController> _logger;
        private readonly IEcsServiceParent _parentService;
        private readonly IEcsCheckService _checkService;
        private readonly IConfiguration _config;
        private readonly ISearchSchoolsUseCase _searchSchoolsUseCase;
        private readonly ICreateUserUseCase _createUserUseCase;
        private readonly ILoadParentDetailsUseCase _loadParentDetailsUseCase;
        private readonly IProcessParentDetailsUseCase _processParentDetailsUseCase;
        private readonly ILoadParentNassDetailsUseCase _loadParentNassDetailsUseCase;
        private readonly ILoaderUseCase _loaderUseCase;

        public CheckController(
           ILogger<CheckController> logger,
        IEcsServiceParent ecsParentService,
        IEcsCheckService ecsCheckService,
        IConfiguration configuration,
        ISearchSchoolsUseCase searchSchoolsUseCase,
        ILoadParentDetailsUseCase loadParentDetailsUseCase,
        ICreateUserUseCase createUserUseCase,
        IProcessParentDetailsUseCase processParentDetailsUseCase,
        ILoadParentNassDetailsUseCase loadParentNassDetailsUseCase,
        ILoaderUseCase loaderUseCase)
        {
            _config = configuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentService = ecsParentService ?? throw new ArgumentNullException(nameof(ecsParentService));
            _checkService = ecsCheckService ?? throw new ArgumentNullException(nameof(ecsCheckService));
            _searchSchoolsUseCase = searchSchoolsUseCase ?? throw new ArgumentNullException(nameof(searchSchoolsUseCase));
            _createUserUseCase = createUserUseCase ?? throw new ArgumentNullException(nameof(createUserUseCase));
            _loadParentDetailsUseCase = loadParentDetailsUseCase ?? throw new ArgumentNullException(nameof(loadParentDetailsUseCase));
            _processParentDetailsUseCase = processParentDetailsUseCase ?? throw new ArgumentNullException(nameof(processParentDetailsUseCase));
            _loadParentNassDetailsUseCase = loadParentNassDetailsUseCase ?? throw new ArgumentNullException(nameof(loadParentNassDetailsUseCase));
            _loaderUseCase = loaderUseCase ?? throw new ArgumentNullException(nameof(loaderUseCase));

            _logger.LogInformation("controller log info");
        }

        [HttpGet]
        public async Task<IActionResult> Enter_Details()
        {
            var viewModel = await _loadParentDetailsUseCase.ExecuteAsync(
                TempData["ParentDetails"]?.ToString(),
                TempData["Errors"]?.ToString()
            );

            if (viewModel.ValidationErrors != null)
            {
                foreach (var (key, errorList) in viewModel.ValidationErrors)
                {
                    foreach (var error in errorList)
                    {
                        ModelState.AddModelError(key, error);
                    }
                }
            }

            return View(viewModel.Parent);
        }

        [HttpPost]
        public async Task<IActionResult> Enter_Details(Parent request)
        {
            if (!ModelState.IsValid)
            {
                TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList());
                TempData["Errors"] = JsonConvert.SerializeObject(errors);

                return RedirectToAction("Enter_Details");
            }

            var (isValid, response, redirectAction) = await _processParentDetailsUseCase.ExecuteAsync(request, HttpContext.Session);

            if (!isValid)
            {
                TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                return RedirectToAction(redirectAction);
            }

            TempData["Response"] = JsonConvert.SerializeObject(response);
            return RedirectToAction(redirectAction);
        }



        public IActionResult Nass()
        {
            var parentDetailsJson = TempData["ParentDetails"] as string;
            if (string.IsNullOrEmpty(parentDetailsJson))
            {
                return RedirectToAction("Enter_Details");
            }

            var parent = _loadParentNassDetailsUseCase.ExecuteAsync(parentDetailsJson).Result;

            return View(parent);
        }


        public async Task<IActionResult> Loader()
        {
            var responseJson = TempData["Response"] as string;
            var (viewName, model) = await _loaderUseCase.ExecuteAsync(responseJson);

            if (viewName == "Loader")
            {
                // Save the response back to TempData for the next poll
                TempData["Response"] = responseJson;
            }

            return View(viewName, model);
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
            try
            {
                string email = HttpContext.User.Claims.First(c => c.Type == "email").Value;
                string uniqueId = HttpContext.User.Claims.First(c => c.Type == "sub").Value;

                var userId = await _createUserUseCase.ExecuteAsync(email, uniqueId);

                HttpContext.Session.SetString("Email", email);
                HttpContext.Session.SetString("UserId", userId);

                return RedirectToAction("Enter_Child_Details");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return View("Outcome/Technical_Error");
            }
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

                    if (schools != null)
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
        public async Task<IActionResult> SearchSchools(string query)
        {
            try
            {
                var schools = await _searchSchoolsUseCase.ExecuteAsync(query);
                return Json(schools.ToList());
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid school search query: {Query}", query);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching schools for query: {Query}", query);
                return BadRequest("An error occurred while searching for schools.");
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
                        Establishment = int.Parse(child.School.URN),
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
