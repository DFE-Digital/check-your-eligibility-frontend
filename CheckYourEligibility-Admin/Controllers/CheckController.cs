using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text;
using Child = CheckYourEligibility_FrontEnd.Models.Child;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    public class CheckController : BaseController
    {

        private readonly ILogger<CheckController> _logger;
        private readonly IEcsCheckService _checkService;
        private readonly IEcsServiceParent _parentService;
        private readonly IConfiguration _config;
        private ILogger<CheckController> _loggerMock;
        private IEcsServiceParent _object;
        DfeClaims? _Claims;

        public CheckController(ILogger<CheckController> logger, IEcsServiceParent ecsServiceParent, IEcsCheckService ecsCheckService, IConfiguration configuration)
        {
            _config = configuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentService = ecsServiceParent ?? throw new ArgumentNullException(nameof(ecsServiceParent));
            _checkService = ecsCheckService ?? throw new ArgumentNullException(nameof(ecsCheckService));
        }

        [HttpGet]
        public IActionResult Enter_Details()
        {
            // start with empty page model
            ParentGuardian request = null;

            // if this page is loaded again after a POST then get the request and update the page with any errors
            if (TempData["ParentDetails"] != null)
            {
                request = JsonConvert.DeserializeObject<ParentGuardian>(TempData["ParentDetails"].ToString());
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
        public async Task<IActionResult> Enter_Details(ParentGuardian request)
        {
            if (request.NinAsrSelection == ParentGuardian.NinAsrSelect.None)
            {
                if (!ModelState.IsValid)
                {
                    // Use PRG pattern so that after this POST the page retrieve informaton from data and performs a GET to avoid browser resubmit confirm error
                    TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList());

                    if (errors.ContainsKey("NationalInsuranceNumber") && errors.ContainsKey("NationalAsylumSeekerServiceNumber"))
                    {
                        string targetValue = "Please select one option";

                        if (errors["NationalInsuranceNumber"].Contains(targetValue) && errors["NationalAsylumSeekerServiceNumber"].Contains(targetValue))
                        {
                            errors.Remove("NationalInsuranceNumber");
                            errors.Remove("NationalAsylumSeekerServiceNumber");
                            errors["NINAS"] = new List<string> { targetValue };
                        }
                    }
                    TempData["Errors"] = JsonConvert.SerializeObject(errors);
                    return RedirectToAction("Enter_Details");
                }
            }

            if (request.NinAsrSelection == ParentGuardian.NinAsrSelect.AsrnSelected)
            {
                ModelState.Remove("NationalInsuranceNumber");

                if (!ModelState.IsValid)
                {
                    // Use PRG pattern so that after this POST the page retrieve informaton from data and performs a GET to avoid browser resubmit confirm error
                    TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList());
                    TempData["Errors"] = JsonConvert.SerializeObject(errors);
                    return RedirectToAction("Enter_Details");
                }

                // build object for api soft-check
                var checkEligibilityRequest = new CheckEligibilityRequest_Fsm()
                {
                    Data = new CheckEligibilityRequestData_Fsm
                    {
                        LastName = request.LastName,
                        NationalAsylumSeekerServiceNumber = request.NationalAsylumSeekerServiceNumber,
                        DateOfBirth = new DateOnly(int.Parse(request.Year), int.Parse(request.Month), int.Parse(request.Day)).ToString("yyyy-MM-dd"),
                    }
                };

                // set important parent details in session storage
                HttpContext.Session.SetString("ParentFirstName", request.FirstName);
                HttpContext.Session.SetString("ParentLastName", request.LastName);
                HttpContext.Session.SetString("ParentDOB", checkEligibilityRequest.Data.DateOfBirth);
                HttpContext.Session.SetString("ParentEmail", request.EmailAddress);

                // set nass detail in session aswell
                HttpContext.Session.SetString("ParentNASS", request.NationalAsylumSeekerServiceNumber);

                // queue api soft-check
                var response = await _checkService.PostCheck(checkEligibilityRequest);

                TempData["Response"] = JsonConvert.SerializeObject(response);

                _logger.LogInformation($"Check processed:- {response.Data.Status} {response.Links.Get_EligibilityCheck}");
            }
            else
            {
                ModelState.Remove("NationalAsylumSeekerServiceNumber");

                if (!ModelState.IsValid)
                {
                    // Use PRG pattern so that after this POST the page retrieve informaton from data and performs a GET to avoid browser resubmit confirm error
                    TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList());
                    TempData["Errors"] = JsonConvert.SerializeObject(errors);
                    return RedirectToAction("Enter_Details");
                }

                // build object for api soft-check
                var checkEligibilityRequest = new CheckEligibilityRequest_Fsm()
                {
                    Data = new CheckEligibilityRequestData_Fsm
                    {
                        LastName = request.LastName,
                        NationalInsuranceNumber = request.NationalInsuranceNumber?.ToUpper(),
                        DateOfBirth = new DateOnly(int.Parse(request.Year), int.Parse(request.Month), int.Parse(request.Day)).ToString("yyyy-MM-dd")
                    }
                };

                // set important parent details in session storage
                HttpContext.Session.SetString("ParentFirstName", request.FirstName);
                HttpContext.Session.SetString("ParentLastName", request.LastName);
                HttpContext.Session.SetString("ParentDOB", checkEligibilityRequest.Data.DateOfBirth);
                HttpContext.Session.SetString("ParentEmail", request.EmailAddress);

                // set nino detail in session aswell
                HttpContext.Session.SetString("ParentNINO", request.NationalInsuranceNumber);

                // queue api soft-check
                var response = await _checkService.PostCheck(checkEligibilityRequest);

                TempData["Response"] = JsonConvert.SerializeObject(response);

                _logger.LogInformation($"Check processed:- {response.Data.Status} {response.Links.Get_EligibilityCheck}");

            }

            return RedirectToAction("Loader");
        }

        public async Task<IActionResult> Loader()
        {
            _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);

            // Retrieve the API response from TempData
            var responseJson = TempData["Response"] as string;
            if (responseJson == null)
            {
                _logger.LogWarning("No response data found in TempData.");
                return View("Outcome/Technical_Error");
            }

            var response = JsonConvert.DeserializeObject<CheckEligibilityResponse>(responseJson);
            _logger.LogInformation($"Check status processed: {response?.Data?.Status}");

            // Call the service to check the current status
            var check = await _checkService.GetStatus(response);
            if (check == null || check.Data == null)
            {
                _logger.LogWarning("Null response received from GetStatus.");
                return View("Outcome/Technical_Error");
            }

            _logger.LogInformation($"Received status: {check.Data.Status}");
            Enum.TryParse(check.Data.Status, out CheckEligibilityStatus status);

            TempData["OutcomeText"] = GetLaOutcomeText(status);

            if (_Claims?.Organisation?.Category?.Name == Constants.CategoryTypeLA)
            {
                return View("Outcome/Eligible_LA");
            }

            else
            {
                TempData["Status"] = GetApplicationRegisteredText(status);
                switch (status)
                {
                    case CheckEligibilityStatus.eligible:
                        return View("Outcome/Eligible");
                    case CheckEligibilityStatus.notEligible:
                        return View("Outcome/Not_Eligible");
                    case CheckEligibilityStatus.parentNotFound:
                        return View("Outcome/Not_Found");
                    case CheckEligibilityStatus.DwpError:
                        return View("Outcome/Technical_Error");
                    case CheckEligibilityStatus.queuedForProcessing:
                        _logger.LogInformation("Still queued for processing.");
                        // Save the response back to TempData for the next poll
                        TempData["Response"] = JsonConvert.SerializeObject(response);
                        // Render the loader view which will auto-refresh
                        return View("Loader");
                    default:
                        _logger.LogError($"Unknown Status {status}");
                        return View("Outcome/Technical_Error");
                }
            }
        }


        private string GetApplicationRegisteredText(CheckEligibilityStatus status)
        {
            switch (status)
            {
                case CheckEligibilityStatus.eligible:
                    return "As these children are entitled to free school meals, you’ll now need to add details of their application to your own system before finalising.";
                case CheckEligibilityStatus.notEligible:
                    return "As these Children are not entitled to free school meals you'll need to add details of the appeal to your own system before finalising";
                default:
                    return "";
            }
        }


        private string GetLaOutcomeText(CheckEligibilityStatus status)
        {
            switch (status)
            {
                case CheckEligibilityStatus.eligible:
                    return "The children of this parent or guardian are entitled to free school meals";
                case CheckEligibilityStatus.notEligible:
                    return "The children of this parent or guardian may not be entitled to free school meals";
                case CheckEligibilityStatus.parentNotFound:
                    return "We could not check if this applicant’s children are entitled to free school meals";
                case CheckEligibilityStatus.DwpError:
                    return "We could not check if this applicant’s children are entitled to free school meals";

                default:
                    return $"Unknown Status {status}";
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

            var fsmApplication = new FsmApplication
            {
                ParentFirstName = HttpContext.Session.GetString("ParentFirstName"),
                ParentLastName = HttpContext.Session.GetString("ParentLastName"),
                ParentDateOfBirth = HttpContext.Session.GetString("ParentDOB"),
                ParentNass = HttpContext.Session.GetString("ParentNASS") ?? null,
                ParentNino = HttpContext.Session.GetString("ParentNINO") ?? null,
                ParentEmail = HttpContext.Session.GetString("ParentEmail"),
                Children = request
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
            try
            {
                // remove child at given index
                var child = request.ChildList[index];
                request.ChildList.Remove(child);

                // set up tempdata so page can be correctly rendered
                TempData["IsChildAddOrRemove"] = true;
                TempData["ChildList"] = JsonConvert.SerializeObject(request.ChildList);

                return RedirectToAction("Enter_Child_Details");
            }
            catch (IndexOutOfRangeException ex)
            {
                throw ex;
            }

        }

        public IActionResult Check_Answers()
        {
            return View("Check_Answers");
        }

        [HttpPost]
        public async Task<IActionResult> Check_Answers(FsmApplication request)
        {
            _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);
            var user = await _parentService.CreateUser(new UserCreateRequest { Data = new UserData { Email = _Claims.User.Email, Reference = _Claims.User.Id } });
            var parentName = $"{request.ParentFirstName} {request.ParentLastName}";
            var response = new ApplicationConfirmationEntitledViewModel { ParentName = parentName, Children = new List<ApplicationConfirmationEntitledChildViewModel>() };

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
                        ParentEmail = request.ParentEmail,
                        ParentDateOfBirth = request.ParentDateOfBirth,
                        ParentNationalInsuranceNumber = request.ParentNino,
                        ParentNationalAsylumSeekerServiceNumber = request.ParentNass,
                        ChildFirstName = child.FirstName,
                        ChildLastName = child.LastName,
                        ChildDateOfBirth = new DateOnly(int.Parse(child.Year), int.Parse(child.Month), int.Parse(child.Day)).ToString("yyyy-MM-dd"),
                        Establishment = int.Parse(_Claims.Organisation.Urn),
                        UserId = user.Data
                    }
                };

                // Send each application individually
                var responseApplication = await _parentService.PostApplication_Fsm(fsmApplication);
                response.Children.Add(new ApplicationConfirmationEntitledChildViewModel
                { ParentName = parentName, ChildName = $"{responseApplication.Data.ChildFirstName} {responseApplication.Data.ChildLastName}", Reference = responseApplication.Data.Reference });
            }
            TempData["confirmationApplication"] = JsonConvert.SerializeObject(response);
            return RedirectToAction("ApplicationsRegistered");
        }

        [HttpGet]
        public IActionResult ApplicationsRegistered()
        {
            var vm = JsonConvert.DeserializeObject<ApplicationConfirmationEntitledViewModel>(TempData["confirmationApplication"].ToString());
            return View("ApplicationsRegistered", vm);
        }

        public IActionResult ChangeChildDetails(int child)
        {
            // set up tempdata and access existing temp data object
            TempData["IsRedirect"] = true;
            TempData["childIndex"] = child;
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