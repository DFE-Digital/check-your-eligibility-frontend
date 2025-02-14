using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_FrontEnd.Controllers;
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
        private readonly IAdminEnterChildDetailsUseCase _adminEnterChildDetailsUseCase;
        private readonly IAdminProcessChildDetailsUseCase _adminProcessChildDetailsUseCase;
        private readonly IAdminAddChildUseCase _adminAddChildUseCase;
        private readonly IAdminLoaderUseCase _adminLoaderUseCase;
        private readonly IAdminRemoveChildUseCase _adminRemoveChildUseCase;
        private readonly IAdminChangeChildDetailsUseCase _adminChangeChildDetailsUseCase;
        private readonly IAdminRegistrationResponseUseCase _adminRegistrationResponseUseCase;
        private readonly IAdminApplicationsRegisteredUseCase _adminApplicationsRegisteredUseCase;
        private readonly IAdminCreateUserUseCase _adminCreateUserUseCase;
        private readonly IAdminSubmitApplicationUseCase _adminSubmitApplicationUseCase;
        private readonly IAdminValidateParentDetailsUseCase _adminValidateParentDetailsUseCase;
        private readonly IAdminInitializeCheckAnswersUseCase _adminInitializeCheckAnswersUseCase;
        public CheckController(
            ILogger<CheckController> logger,
            IEcsServiceParent ecsServiceParent,
            IEcsCheckService ecsCheckService,
            IConfiguration configuration,
            IAdminLoadParentDetailsUseCase adminLoadParentDetailsUseCase,
            IAdminProcessParentDetailsUseCase adminProcessParentDetailsUseCase,
            IAdminEnterChildDetailsUseCase adminEnterChildDetailsUseCase,
            IAdminProcessChildDetailsUseCase adminProcessChildDetailsUseCase,
            IAdminAddChildUseCase adminAddChildUseCase,
            IAdminLoaderUseCase adminLoaderUseCase,
            IAdminRemoveChildUseCase adminRemoveChildUseCase,
            IAdminChangeChildDetailsUseCase adminChangeChildDetailsUseCase,
            IAdminRegistrationResponseUseCase adminRegistrationResponseUseCase,
            IAdminApplicationsRegisteredUseCase adminApplicationsRegisteredUseCase,
            IAdminCreateUserUseCase adminCreateUserUseCase,
            IAdminSubmitApplicationUseCase adminSubmitApplicationUseCase,
            IAdminValidateParentDetailsUseCase adminValidateParentDetailsUseCase,
            IAdminInitializeCheckAnswersUseCase adminInitializeCheckAnswersUseCase)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentService = ecsServiceParent ?? throw new ArgumentNullException(nameof(ecsServiceParent));
            _checkService = ecsCheckService ?? throw new ArgumentNullException(nameof(ecsCheckService));
            _adminLoadParentDetailsUseCase = adminLoadParentDetailsUseCase ?? throw new ArgumentNullException(nameof(adminLoadParentDetailsUseCase));
            _adminProcessParentDetailsUseCase = adminProcessParentDetailsUseCase ?? throw new ArgumentNullException(nameof(adminProcessParentDetailsUseCase));
            _adminEnterChildDetailsUseCase = adminEnterChildDetailsUseCase ?? throw new ArgumentNullException(nameof(adminEnterChildDetailsUseCase));
            _adminProcessChildDetailsUseCase = adminProcessChildDetailsUseCase ?? throw new ArgumentNullException(nameof(adminProcessChildDetailsUseCase));
            _adminAddChildUseCase = adminAddChildUseCase ?? throw new ArgumentNullException(nameof(adminAddChildUseCase));
            _adminLoaderUseCase = adminLoaderUseCase ?? throw new ArgumentNullException(nameof(adminLoaderUseCase));
            _adminRemoveChildUseCase = adminRemoveChildUseCase ?? throw new ArgumentNullException(nameof(adminRemoveChildUseCase));
            _adminChangeChildDetailsUseCase = adminChangeChildDetailsUseCase ?? throw new ArgumentNullException(nameof(adminChangeChildDetailsUseCase));
            _adminRegistrationResponseUseCase = adminRegistrationResponseUseCase ?? throw new ArgumentNullException(nameof(adminRegistrationResponseUseCase));
            _adminApplicationsRegisteredUseCase = adminApplicationsRegisteredUseCase ?? throw new ArgumentNullException(nameof(adminApplicationsRegisteredUseCase));
            _adminCreateUserUseCase = adminCreateUserUseCase ?? throw new ArgumentNullException(nameof(adminCreateUserUseCase));
            _adminSubmitApplicationUseCase = adminSubmitApplicationUseCase ?? throw new ArgumentNullException(nameof(adminSubmitApplicationUseCase));
            _adminValidateParentDetailsUseCase = adminValidateParentDetailsUseCase ?? throw new ArgumentNullException(nameof(adminValidateParentDetailsUseCase));
            _adminInitializeCheckAnswersUseCase = adminInitializeCheckAnswersUseCase ?? throw new ArgumentNullException(nameof(adminInitializeCheckAnswersUseCase));
        }

        [HttpGet]
        public async Task<IActionResult> Enter_Details()
        {
            try
            {
                if (TempData["Response"] != null)
                {
                    return RedirectToAction("Loader");
                }

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
                var validationResult = _adminValidateParentDetailsUseCase.Execute(request, ModelState);

                if (!validationResult.IsValid)
                {
                    TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                    TempData["Errors"] = JsonConvert.SerializeObject(validationResult.Errors);
                    return RedirectToAction("Enter_Details");
                }

                var response = await _adminProcessParentDetailsUseCase.Execute(request, HttpContext.Session);
                TempData["Response"] = JsonConvert.SerializeObject(response);

                return RedirectToAction("Loader");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing parent details");
                return View("Outcome/Technical_Error");
            }
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

            // Execute the use case.
            var result = await _adminLoaderUseCase.ExecuteAsync(responseJson, HttpContext.User);

            // If the status is queued for processing, put the response back into TempData.
            if (result.UpdatedResponseJson != null)
            {
                TempData["Response"] = result.UpdatedResponseJson;
            }

            // IMPORTANT: Set OutcomeStatus so that your tests can verify it.
            TempData["OutcomeStatus"] = result.Status;

            return View(result.ViewName);
        }


        [HttpGet]
        public IActionResult Enter_Child_Details()
        {
            var children = new Children() { ChildList = [new()] };

            if (TempData["IsChildAddOrRemove"] != null && (bool)TempData["IsChildAddOrRemove"] == true)
            {
                ModelState.Clear();

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
                return View("Enter_Child_Details", request);
            }

            if (!ModelState.IsValid)
            {
                return View("Enter_Child_Details", request);
            }

            var fsmApplication = _adminProcessChildDetailsUseCase.Execute(request, HttpContext.Session).Result;
            TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);

            return View("Check_Answers", fsmApplication);
        }

        [HttpPost]
        public IActionResult Add_Child(Children request)
        {
            TempData["IsChildAddOrRemove"] = true;

            var result = _adminAddChildUseCase.Execute(request);

            TempData["ChildList"] = JsonConvert.SerializeObject(result.ChildList);

            return RedirectToAction("Enter_Child_Details");
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
            catch (IndexOutOfRangeException)
            {
                throw;
            }
        }


        [HttpPost]
        public async Task<IActionResult> Check_Answers(FsmApplication request)
        {
            _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);
            var userId = await _adminCreateUserUseCase.Execute(HttpContext.User.Claims);

            var (result, lastResponse) = await _adminSubmitApplicationUseCase.Execute(
                request,
                userId,
                _Claims.Organisation.Urn);

            TempData["confirmationApplication"] = JsonConvert.SerializeObject(result);

            return RedirectToAction(
                lastResponse.Data.Status == "Entitled"
                    ? "ApplicationsRegistered"
                    : "AppealsRegistered");
        }

        public IActionResult Check_Answers()
        {
            return View("Check_Answers");
        }


        public IActionResult ChangeChildDetails(int child)
        {
            TempData["IsRedirect"] = true;
            TempData["childIndex"] = child;
            var responseJson = TempData["FsmApplication"] as string;
            var children = _adminChangeChildDetailsUseCase.Execute(responseJson);
            return View("Enter_Child_Details", children);
            return View("Enter_Child_Details", children);
        }


        [HttpGet]
        public IActionResult ApplicationsRegistered()
        {
            var vm = JsonConvert.DeserializeObject<ApplicationConfirmationEntitledViewModel>(TempData["confirmationApplication"].ToString());
            return View("ApplicationsRegistered", vm);
        }


        [HttpGet]
        public IActionResult AppealsRegistered()
        {
            var vm = JsonConvert.DeserializeObject<ApplicationConfirmationEntitledViewModel>(TempData["confirmationApplication"].ToString());
            return View("AppealsRegistered", vm);
        }
    }
}