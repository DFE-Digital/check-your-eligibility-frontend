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
        private readonly IParentSignInUseCase _parentSignInUseCase;
        private readonly IEnterChildDetailsUseCase _enterChildDetailsUseCase;
        private readonly IProcessChildDetailsUseCase _processChildDetailsUseCase;
        private readonly IAddChildUseCase _addChildUseCase;
        private readonly IRemoveChildUseCase _removeChildUseCase;
        private readonly ICheckAnswersUseCase _checkAnswersUseCase;
        private readonly IApplicationSentUseCase _applicationSentUseCase;
        private readonly IChangeChildDetailsUseCase _changeChildDetailsUseCase;
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
        ILoaderUseCase loaderUseCase,
        IParentSignInUseCase parentSignInUseCase,
        IEnterChildDetailsUseCase enterChildDetailsUseCase,
        IProcessChildDetailsUseCase processChildDetailsUseCase,
        IAddChildUseCase addChildUseCase,
        IRemoveChildUseCase removeChildUseCase,
        ICheckAnswersUseCase checkAnswersUseCase,
        IApplicationSentUseCase applicationSentUseCase,
        IChangeChildDetailsUseCase changeChildDetailsUseCase)

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
            _parentSignInUseCase = parentSignInUseCase ?? throw new ArgumentNullException(nameof(parentSignInUseCase));
            _enterChildDetailsUseCase = enterChildDetailsUseCase ?? throw new ArgumentNullException(nameof(enterChildDetailsUseCase));
            _processChildDetailsUseCase = processChildDetailsUseCase ?? throw new ArgumentNullException(nameof(processChildDetailsUseCase));
            _addChildUseCase = addChildUseCase ?? throw new ArgumentNullException(nameof(addChildUseCase));
            _removeChildUseCase = removeChildUseCase ?? throw new ArgumentNullException(nameof(removeChildUseCase));
            _checkAnswersUseCase = checkAnswersUseCase ?? throw new ArgumentNullException(nameof(checkAnswersUseCase));
            _applicationSentUseCase = applicationSentUseCase ?? throw new ArgumentNullException(nameof(applicationSentUseCase));
            _changeChildDetailsUseCase = changeChildDetailsUseCase ?? throw new ArgumentNullException(nameof(changeChildDetailsUseCase));

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
            var (viewName, model) = await _loaderUseCase.ExecuteAsync(responseJson, HttpContext.Session);

            if (viewName == "Loader")
            {
                // Save the response back to TempData for the next poll
                TempData["Response"] = responseJson;
            }

            return View(viewName, model);
        }

        public async Task<IActionResult> SignIn()
        {
            var properties = await _parentSignInUseCase.ExecuteAsync("/Check/CreateUser");
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

        public async Task<IActionResult> Enter_Child_Details()
        {
            var childrenModel = await _enterChildDetailsUseCase.ExecuteAsync(
                TempData["ChildList"]?.ToString(),
                TempData["IsChildAddOrRemove"] as bool?);

            return View(childrenModel);
        }

        [HttpPost]
        public async Task<IActionResult> Enter_Child_Details(Children request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var isRedirect = TempData["FsmApplication"] != null &&
                           TempData["IsRedirect"] != null &&
                           (bool)TempData["IsRedirect"] == true;

            var validationErrors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    k => k.Key,
                    v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());

            var (isSuccess, view, model, errors) = await _processChildDetailsUseCase.ExecuteAsync(
                request,
                isRedirect,
                HttpContext.Session,
                validationErrors);

            if (!isSuccess)
            {
                foreach (var error in errors)
                {
                    foreach (var message in error.Value)
                    {
                        ModelState.AddModelError(error.Key, message);
                    }
                }
                return View(model);
            }

            if (model is FsmApplication fsmApplication)
            {
                TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);
            }

            return View(view, model);
        }

        [HttpPost]
        public async Task<IActionResult> Add_Child(Children request)
        {
            var (isSuccess, updatedChildren) = await _addChildUseCase.ExecuteAsync(request);

            TempData["IsChildAddOrRemove"] = true;

            if (!isSuccess)
            {
                return RedirectToAction("Enter_Child_Details");
            }

            TempData["ChildList"] = JsonConvert.SerializeObject(updatedChildren.ChildList);
            return RedirectToAction("Enter_Child_Details");
        }

        [HttpPost]
        public async Task<IActionResult> Remove_Child(Children request, int index)
        {
            var (isSuccess, updatedChildren, errorMessage) = await _removeChildUseCase.ExecuteAsync(request, index);

            TempData["IsChildAddOrRemove"] = true;

            if (!isSuccess)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return RedirectToAction("Enter_Child_Details");
            }

            TempData["ChildList"] = JsonConvert.SerializeObject(updatedChildren.ChildList);
            return RedirectToAction("Enter_Child_Details");
        }

        /// this method is called by AJAX
        [HttpGet]
        public async Task<IActionResult> SearchSchools(string query)
        {
            try
            {
                // Sanitize input before processing
                string sanitizedQuery = query?.Trim()
                    .Replace(Environment.NewLine, "")
                    .Replace("\n", "")
                    .Replace("\r", "")
                    // Add more sanitization as needed
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");

                if (string.IsNullOrEmpty(sanitizedQuery) || sanitizedQuery.Length < 3)
                {
                    _logger.LogWarning("Invalid school search query: {Query}", sanitizedQuery);
                    return BadRequest("Query must be at least 3 characters long.");
                }

                var schools = await _searchSchoolsUseCase.ExecuteAsync(sanitizedQuery);
                return Json(schools.ToList());
            }
            catch (Exception ex)
            {
                // Log sanitized query only
                _logger.LogError(ex, "Error searching schools for query: {Query}",
                    query?.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));
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
            try
            {
                var currentStatus = HttpContext.Session.GetString("CheckResult");
                var userId = HttpContext.Session.GetString("UserId");
                var email = HttpContext.Session.GetString("Email");

                var (isSuccess, responses, errorMessage) = await _checkAnswersUseCase.ProcessApplicationAsync(
                    request, currentStatus, userId, email);

                if (!isSuccess)
                {
                    _logger.LogError("Application processing failed: {Error}", errorMessage);
                    throw new Exception(errorMessage);
                }

                TempData["FsmApplicationResponses"] = JsonConvert.SerializeObject(responses);
                return RedirectToAction("Application_Sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Check_Answers");
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Application_Sent()
        {
            ModelState.Clear();
            var (viewName, model) = await _applicationSentUseCase.ExecuteAsync();
            return View(viewName, model);
        }

        public async Task<IActionResult> ChangeChildDetails()
        {
            TempData["IsRedirect"] = true;
            (bool isSuccess, string viewName, Children model) = await _changeChildDetailsUseCase.ExecuteAsync(
                TempData["FsmApplication"] as string);

            return View(viewName, model);
        }


    }
}
