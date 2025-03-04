﻿using CheckYourEligibility.Domain.Enums;
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
        private readonly IPerformEligibilityCheckUseCase _performEligibilityCheckUseCase;
        private readonly IGetCheckStatusUseCase _getCheckStatusUseCase;
        private readonly ISignInUseCase _signInUseCase;
        private readonly IEnterChildDetailsUseCase _enterChildDetailsUseCase;
        private readonly IProcessChildDetailsUseCase _processChildDetailsUseCase;
        private readonly IAddChildUseCase _addChildUseCase;
        private readonly IRemoveChildUseCase _removeChildUseCase;
        private readonly ISubmitApplicationUseCase _submitApplicationUseCase;
        private readonly IChangeChildDetailsUseCase _changeChildDetailsUseCase;
        public CheckController(
           ILogger<CheckController> logger,
        IEcsServiceParent ecsParentService,
        IEcsCheckService ecsCheckService,
        IConfiguration configuration,
        ISearchSchoolsUseCase searchSchoolsUseCase,
        ILoadParentDetailsUseCase loadParentDetailsUseCase,
        ICreateUserUseCase createUserUseCase,
        IPerformEligibilityCheckUseCase performEligibilityCheckUseCase,
        IGetCheckStatusUseCase getCheckStatusUseCase,
        ISignInUseCase signInUseCase,
        IEnterChildDetailsUseCase enterChildDetailsUseCase,
        IProcessChildDetailsUseCase processChildDetailsUseCase,
        IAddChildUseCase addChildUseCase,
        IRemoveChildUseCase removeChildUseCase,
        ISubmitApplicationUseCase submitApplicationUseCase,
        IChangeChildDetailsUseCase changeChildDetailsUseCase)

        {
            _config = configuration;
            _logger = logger;
            _parentService = ecsParentService;
            _checkService = ecsCheckService;
            _searchSchoolsUseCase = searchSchoolsUseCase;
            _createUserUseCase = createUserUseCase;
            _loadParentDetailsUseCase = loadParentDetailsUseCase;
            _performEligibilityCheckUseCase = performEligibilityCheckUseCase;
            _getCheckStatusUseCase = getCheckStatusUseCase;
            _signInUseCase = signInUseCase;
            _enterChildDetailsUseCase = enterChildDetailsUseCase;
            _processChildDetailsUseCase = processChildDetailsUseCase;
            _addChildUseCase = addChildUseCase;
            _removeChildUseCase = removeChildUseCase;
            _submitApplicationUseCase = submitApplicationUseCase;
            _changeChildDetailsUseCase = changeChildDetailsUseCase;

            _logger.LogInformation("controller log info");
        }

        [HttpGet]
        public async Task<IActionResult> Enter_Details()
        {
            var viewModel = await _loadParentDetailsUseCase.Execute(
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

            var (response, responseCode) = await _performEligibilityCheckUseCase.Execute(request, HttpContext.Session);

            switch (responseCode) {
                case "Success":
                    TempData["Response"] = JsonConvert.SerializeObject(response);
                    return RedirectToAction("Loader");
                
                case "Nass":
                    TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                    return RedirectToAction("Nass");
                
                default:
                    return View("Outcome/Could_Not_Check");
            }
        }



        public IActionResult Nass()
        {
            var parentDetailsJson = TempData["ParentDetails"] as string;
            if (string.IsNullOrEmpty(parentDetailsJson))
            {
                return RedirectToAction("Enter_Details");
            }

            var parent = JsonConvert.DeserializeObject<Parent>(parentDetailsJson)??new Parent();

            return View(parent);
        }


        public async Task<IActionResult> Loader()
        {
            var responseJson = TempData["Response"] as string;
            try
            {
                string outcome = await _getCheckStatusUseCase.Execute(responseJson, HttpContext.Session);

                if (outcome == "queuedForProcessing")
                {
                    // Save the response back to TempData for the next poll
                    TempData["Response"] = responseJson;
                }

                _logger.LogError(outcome);

                switch (outcome)
                {
                    case "eligible":
                        return View("Outcome/Eligible");
                        break;

                    case "notEligible":
                        return View("Outcome/Not_Eligible");
                        break;

                    case "parentNotFound":
                        return View("Outcome/Not_Found");
                        break;

                    case "queuedForProcessing":
                        return View("Loader");
                        break;

                    default:
                        return View("Outcome/Technical_Error");
                }
            }
            catch (Exception ex)
            {
                return View("Outcome/Technical_Error");
            }
        }

        public async Task<IActionResult> SignIn()
        {
            var properties = await _signInUseCase.Execute("/Check/CreateUser");
            return Challenge(properties, authenticationSchemes: OneLoginDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> CreateUser()
        {
            try
            {
                string email = HttpContext.User.Claims.First(c => c.Type == "email").Value;
                string uniqueId = HttpContext.User.Claims.First(c => c.Type == "sub").Value;

                var userId = await _createUserUseCase.Execute(email, uniqueId);

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
            var childrenModel = _enterChildDetailsUseCase.Execute(
                TempData["ChildList"] as string,
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

            if (isRedirect)
            {
                return View("Enter_Child_Details", request);
            }
            
            try
            {
                var model = await _processChildDetailsUseCase.Execute(
                    request,
                    HttpContext.Session,
                    validationErrors);

                if (model is FsmApplication fsmApplication)
                {
                    TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);
                }
                return View("Check_Answers", model);
            }
            
            catch (ProcessChildDetailsUseCase.ProcessChildDetailsValidationException e)
            {
                Dictionary<string, string[]> errors = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(e.Message);
                foreach (var error in errors)
                {
                    foreach (var message in error.Value)
                    {
                        ModelState.AddModelError(error.Key, message);
                    }
                }

                return View("Enter_Child_Details");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add_Child(Children request)
        {
            try
            {
                TempData["IsChildAddOrRemove"] = true;
                
                Children updatedChildren = _addChildUseCase.Execute(request);

                TempData["ChildList"] = JsonConvert.SerializeObject(updatedChildren.ChildList);
            }
            catch (MaxChildrenException e)
            {
                TempData["ChildList"] = JsonConvert.SerializeObject(request.ChildList);
            }

            return RedirectToAction("Enter_Child_Details");
        }

        [HttpPost]
        public async Task<IActionResult> Remove_Child(Children request, int index)
        {
            try
            {
                TempData["IsChildAddOrRemove"] = true;
                
                var updatedChildren = _removeChildUseCase.Execute(request, index);

                TempData["ChildList"] = JsonConvert.SerializeObject(updatedChildren.ChildList);
                
                return RedirectToAction("Enter_Child_Details");
            }

            catch (RemoveChildValidationException e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return RedirectToAction("Enter_Child_Details");
            }
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

                var schools = await _searchSchoolsUseCase.Execute(sanitizedQuery);
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
            var currentStatus = HttpContext.Session.GetString("CheckResult");
            var userId = HttpContext.Session.GetString("UserId");
            var email = HttpContext.Session.GetString("Email");

            var responses = await _submitApplicationUseCase.Execute(
                request, currentStatus, userId, email);

            TempData["FsmApplicationResponses"] = JsonConvert.SerializeObject(responses);
            return RedirectToAction("Application_Sent");
        }

        [HttpGet]
        public async Task<IActionResult> Application_Sent()
        {
            ModelState.Clear();
            return View("Application_Sent");
        }

        public async Task<IActionResult> ChangeChildDetails()
        {
            TempData["IsRedirect"] = true;
            Children model = new Children { ChildList = new List<Child>() };
            
            try
            {
                model = _changeChildDetailsUseCase.Execute(
                    TempData["FsmApplication"] as string);
            }
            catch (JSONException e)
            {
                ;
            }
            catch (NoChildException)
            {
                ;
            }

            return View("Enter_Child_Details", model);
        }


    }
}
