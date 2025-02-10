using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    public class CheckController : BaseController
    {
        private readonly ILogger<CheckController> _logger;
        private readonly IEcsCheckService _checkService;
        private readonly IEcsServiceParent _parentService;
        private readonly IConfiguration _config;
        private readonly IAdminLoadParentDetailsUseCase _adminLoadParentDetailsUseCase;
        private readonly IAdminProcessParentDetailsUseCase _adminProcessParentDetailsUseCase;
        private readonly IAdminLoaderUseCase _adminLoaderUseCase;
        private readonly IAdminEnterChildDetailsUseCase _adminEnterChildDetailsUseCase;
        private readonly IAdminProcessChildDetailsUseCase _adminProcessChildDetailsUseCase;
        private readonly IAdminAddChildUseCase _adminAddChildUseCase;
        private readonly IAdminRemoveChildUseCase _adminRemoveChildUseCase;
        private readonly IAdminChangeChildDetailsUseCase _adminChangeChildDetailsUseCase;
        private readonly IAdminRegistrationResponseUseCase _adminRegistrationResponseUseCase;
        private readonly IAdminApplicationsRegisteredUseCase _adminApplicationsRegisteredUseCase;

        public CheckController(
            ILogger<CheckController> logger,
            IEcsServiceParent ecsServiceParent,
            IEcsCheckService ecsCheckService,
            IConfiguration configuration,
            IAdminLoadParentDetailsUseCase adminLoadParentDetailsUseCase,
            IAdminProcessParentDetailsUseCase adminProcessParentDetailsUseCase,
            IAdminLoaderUseCase adminLoaderUseCase,
            IAdminEnterChildDetailsUseCase adminEnterChildDetailsUseCase,
            IAdminProcessChildDetailsUseCase adminProcessChildDetailsUseCase,
            IAdminAddChildUseCase adminAddChildUseCase,
            IAdminRemoveChildUseCase adminRemoveChildUseCase,
            IAdminChangeChildDetailsUseCase adminChangeChildDetailsUseCase,
            IAdminRegistrationResponseUseCase adminRegistrationResponseUseCase,
            IAdminApplicationsRegisteredUseCase adminApplicationsRegisteredUseCase)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentService = ecsServiceParent ?? throw new ArgumentNullException(nameof(ecsServiceParent));
            _checkService = ecsCheckService ?? throw new ArgumentNullException(nameof(ecsCheckService));
            _adminLoadParentDetailsUseCase = adminLoadParentDetailsUseCase ?? throw new ArgumentNullException(nameof(adminLoadParentDetailsUseCase));
            _adminProcessParentDetailsUseCase = adminProcessParentDetailsUseCase ?? throw new ArgumentNullException(nameof(adminProcessParentDetailsUseCase));
            _adminLoaderUseCase = adminLoaderUseCase ?? throw new ArgumentNullException(nameof(adminLoaderUseCase));
            _adminEnterChildDetailsUseCase = adminEnterChildDetailsUseCase ?? throw new ArgumentNullException(nameof(adminEnterChildDetailsUseCase));
            _adminProcessChildDetailsUseCase = adminProcessChildDetailsUseCase ?? throw new ArgumentNullException(nameof(adminProcessChildDetailsUseCase));
            _adminAddChildUseCase = adminAddChildUseCase ?? throw new ArgumentNullException(nameof(adminAddChildUseCase));
            _adminRemoveChildUseCase = adminRemoveChildUseCase ?? throw new ArgumentNullException(nameof(adminRemoveChildUseCase));
            _adminChangeChildDetailsUseCase = adminChangeChildDetailsUseCase ?? throw new ArgumentNullException(nameof(adminChangeChildDetailsUseCase));
            _adminRegistrationResponseUseCase = adminRegistrationResponseUseCase ?? throw new ArgumentNullException(nameof(adminRegistrationResponseUseCase));
            _adminApplicationsRegisteredUseCase = adminApplicationsRegisteredUseCase ?? throw new ArgumentNullException(nameof(adminApplicationsRegisteredUseCase));
        }

        [HttpGet]
        public async Task<IActionResult> Enter_Details()
        {
            try
            {
                var (parent, validationErrors) = await _adminLoadParentDetailsUseCase.Execute(
                    TempData["ParentDetails"]?.ToString(),
                    TempData["Errors"]?.ToString()
                );

                if (validationErrors != null)
                {
                    foreach (var (key, errorList) in validationErrors)
                    {
                        foreach (var error in errorList)
                        {
                            ModelState.AddModelError(key, error);
                        }
                    }
                }

                return View(parent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading parent details");
                return View("Outcome/Technical_Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Enter_Details(ParentGuardian request)
        {
            try
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

                var result = await _adminProcessParentDetailsUseCase.Execute(request, HttpContext.Session);
                TempData["Response"] = JsonConvert.SerializeObject(result.Response);
                return RedirectToAction(result.RedirectAction);
            }
            catch (AdminParentDetailsValidationException ex)
            {
                TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                TempData["Errors"] = ex.Message;
                return RedirectToAction("Enter_Details");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing parent details");
                return View("Outcome/Technical_Error");
            }
        }

        public async Task<IActionResult> Loader()
        {
            try
            {
                var responseJson = TempData["Response"] as string;
                var result = await _adminLoaderUseCase.Execute(responseJson, HttpContext.User.Claims);

                if (result.Model != null)
                {
                    TempData["OutcomeStatus"] = result.Model;
                }

                if (result.ViewName == "Loader")
                {
                    TempData["Response"] = responseJson;
                }

                return View(result.ViewName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in loader");
                return View("Outcome/Technical_Error");
            }
        }


        public async Task<IActionResult> Enter_Child_Details()
        {
            try
            {
                var children = await _adminEnterChildDetailsUseCase.Execute(
                    TempData["ChildList"]?.ToString(),
                    TempData["IsChildAddOrRemove"] as bool?
                );

                return View(children);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error entering child details");
                return View("Outcome/Technical_Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Enter_Child_Details(Children request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var validationErrors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    k => k.Key,
                    v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());

            try
            {
                var model = await _adminProcessChildDetailsUseCase.Execute(request, HttpContext.Session, validationErrors);
                if (model is FsmApplication fsmApplication)
                {
                    TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);
                    return View("Check_Answers", model);
                }
                ModelState.AddModelError("", "Invalid response from service");
                return View(request);
            }
            catch (AdminProcessChildDetailsException e)
            {
                ModelState.AddModelError("", e.Message);
                return View(request);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add_Child(Children request)
        {
            try
            {
                var result = await _adminAddChildUseCase.Execute(request);
                TempData["IsChildAddOrRemove"] = true;
                TempData["ChildList"] = JsonConvert.SerializeObject(result.ChildList);
                return RedirectToAction("Enter_Child_Details");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding child");
                return View("Outcome/Technical_Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Remove_Child(Children request, int index)
        {
            try
            {
                var result = await _adminRemoveChildUseCase.Execute(request, index);
                TempData["IsChildAddOrRemove"] = true;
                TempData["ChildList"] = JsonConvert.SerializeObject(result.ChildList);
                return RedirectToAction("Enter_Child_Details");
            }
            catch (AdminRemoveChildException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("Enter_Child_Details");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing child");
                return View("Outcome/Technical_Error");
            }
        }

        public IActionResult Check_Answers()
        {
            return View("Check_Answers");
        }

        [HttpPost]
        public async Task<IActionResult> Check_Answers(FsmApplication request)
        {
            try
            {
                var result = await _adminRegistrationResponseUseCase.Execute(request);
                TempData["confirmationApplication"] = JsonConvert.SerializeObject(result);
                return RedirectToAction("ApplicationsRegistered");
            }
            catch (AdminRegistrationResponseException ex)
            {
                _logger.LogError(ex, "Error processing registration");
                ModelState.AddModelError("", ex.Message);
                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in check answers");
                return View("Outcome/Technical_Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ApplicationsRegistered()
        {
            try
            {
                var applicationJson = TempData["confirmationApplication"]?.ToString();
                var result = await _adminApplicationsRegisteredUseCase.Execute(applicationJson);

                if (!result.IsSuccess)
                {
                    return View(result.ErrorViewName);
                }

                // Preserve the TempData if it might be needed later
                TempData.Keep("confirmationApplication");
                return View("ApplicationsRegistered", result.ViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in admin ApplicationsRegistered action");
                return View("Outcome/Technical_Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> AppealsRegistered()
        {
            try
            {
                var applicationJson = TempData["confirmationApplication"]?.ToString();
                var viewModel = JsonConvert.DeserializeObject<ApplicationConfirmationEntitledViewModel>(applicationJson);
                return View("AppealsRegistered", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in appeals registered");
                return View("Outcome/Technical_Error");
            }
        }

        public async Task<IActionResult> ChangeChildDetails(int child)
        {
            try
            {
                TempData["IsRedirect"] = true;
                TempData["childIndex"] = child;
                var responseJson = TempData["FsmApplication"] as string;

                var children = await _adminChangeChildDetailsUseCase.Execute(responseJson);
                if (children?.ChildList == null || !children.ChildList.Any())
                {
                    return RedirectToAction("Enter_Child_Details");
                }

                return View("Enter_Child_Details", children);
            }
            catch (AdminChangeChildDetailsException)
            {
                return RedirectToAction("Enter_Child_Details");
            }
        }
    }
}