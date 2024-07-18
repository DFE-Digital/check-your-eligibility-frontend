using CheckYourEligibility.Domain.Constants;
using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CsvHelper;
using CsvHelper.Configuration;
using FeatureManagement.Domain.Validation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    public class SchoolController : BaseController
    {
        const int TotalErrorsToDisplay = 20;

        private readonly ILogger<SchoolController> _logger;
        private readonly IEcsServiceParent _parentService;
        private readonly IEcsServiceAdmin _adminService;
        private readonly IConfiguration _config;
        private ILogger<SchoolController> _loggerMock;
        private IEcsServiceParent _object;
        DfeClaims? _Claims;
        
        public SchoolController(ILogger<SchoolController> logger, IEcsServiceParent ecsServiceParent, IEcsServiceAdmin ecsServiceAdmin, IConfiguration configuration)
        {
            _config = configuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentService = ecsServiceParent ?? throw new ArgumentNullException(nameof(ecsServiceParent));
            _adminService = ecsServiceAdmin ?? throw new ArgumentNullException(nameof(ecsServiceAdmin));
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
            if (request.IsNassSelected == true)
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
                var checkEligibilityRequest = new CheckYourEligibility.Domain.Requests.CheckEligibilityRequest()
                {
                    Data = new CheckEligibilityRequestDataFsm
                    {
                        LastName = request.LastName,
                        NationalAsylumSeekerServiceNumber = request.NationalAsylumSeekerServiceNumber,
                        DateOfBirth = new DateOnly(request.Year.Value, request.Month.Value, request.Day.Value).ToString("yyyy-MM-dd"),
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
                var response = await _parentService.PostCheck(checkEligibilityRequest);

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
                var checkEligibilityRequest = new CheckYourEligibility.Domain.Requests.CheckEligibilityRequest()
                {
                    Data = new CheckYourEligibility.Domain.Requests.CheckEligibilityRequestDataFsm
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
                HttpContext.Session.SetString("ParentEmail", request.EmailAddress);

                // set nino detail in session aswell
                HttpContext.Session.SetString("ParentNINO", request.NationalInsuranceNumber);

                // queue api soft-check
                var response = await _parentService.PostCheck(checkEligibilityRequest);

                TempData["Response"] = JsonConvert.SerializeObject(response);

                _logger.LogInformation($"Check processed:- {response.Data.Status} {response.Links.Get_EligibilityCheck}");

            }

            return RedirectToAction("Loader");
        }

        public IActionResult Nass()
        {
            var parent = new ParentGuardian();

            return View(parent);
        }

        [HttpPost]
        public async Task<IActionResult> Nass(ParentGuardian request)
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
                var response = await _parentService.PostCheck(checkEligibilityRequest);

                _logger.LogInformation($"Check processed:- {response.Data.Status} {response.Links.Get_EligibilityCheck}");

                // go to loader page which will poll soft-check status
                return RedirectToAction("Loader");
            }
        }

        public IActionResult Loader()
        {
            return View();
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
            // remove child at given index
            var child = request.ChildList[index];
            request.ChildList.Remove(child);

            // set up tempdata so page can be correctly rendered
            TempData["IsChildAddOrRemove"] = true;
            TempData["ChildList"] = JsonConvert.SerializeObject(request.ChildList);

            return RedirectToAction("Enter_Child_Details");
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
                        School = int.Parse(_Claims.Organisation.Urn),
                        UserId = _Claims.User.Id
                    }
                };

                // Send each application as an individual check
                var response = await _parentService.PostApplication(fsmApplication);
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

        public IActionResult Batch_Check()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Batch_Check(IFormFile fileUpload)
        {
            TempData["Response"] = "data_issue";
            List<CheckRow> DataLoad;
            var errorCount = 0;
            var requestItems = new List<CheckEligibilityRequestDataFsm>();
            var validationResultsItems = new StringBuilder();
            if (fileUpload == null || fileUpload.ContentType.ToLower() != "text/csv")
            {
                return BadRequest(new MessageResponse { Data = $"{Admin.CsvfileRequired}" });
            }
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    BadDataFound = null,
                    MissingFieldFound = null
                };
                using (var fileStream = fileUpload.OpenReadStream())

                using (var csv = new CsvReader(new StreamReader(fileStream), config))
                {
                    csv.Context.RegisterClassMap<CheckRowRowMap>();
                    DataLoad = csv.GetRecords<CheckRow>().ToList();

                    if (DataLoad == null || !DataLoad.Any())
                    {
                        throw new InvalidDataException("Invalid file content.");
                    }
                }
                var validator = new CheckEligibilityRequestDataValidator();
                var sequence = 1;
               
                
                foreach (var item in DataLoad)
                {

                    var requestItem = new CheckEligibilityRequestDataFsm()
                    {
                        LastName = item.LastName,
                        DateOfBirth = DateTime.TryParse(item.DOB, out var dtval) ? dtval.ToString("yyyy-MM-dd") : string.Empty,
                        NationalInsuranceNumber = item.Ni.ToUpper(),
                        NationalAsylumSeekerServiceNumber = item.Nass.ToUpper(),

                    };
                    var validationResults = validator.Validate(requestItem);
                    if (!validationResults.IsValid)
                    {
                        errorCount = checkIfExists(sequence, validationResultsItems, validationResults,errorCount);
                    }
                    else
                    {
                        requestItems.Add(requestItem);

                    }
                    sequence++;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("ImportEstablishmentData", ex);
                validationResultsItems.AppendLine(ex.Message);
            }
            if (validationResultsItems.Length > 0)
            {
                if ((errorCount - TotalErrorsToDisplay) > 0)
                {
                    TempData["BatchParentCheckItemsLineMoreErrors"] = errorCount- TotalErrorsToDisplay;
                }

                TempData["BatchParentCheckItemsErrors"] = validationResultsItems.ToString();
                return View("BatchOutcome/Error_Data_Issue");
            }
            else
            {
                var result = await _adminService.PostBulkCheck(new CheckEligibilityRequestBulk { Data = requestItems});
                HttpContext.Session.SetString("Get_Progress_Check", result.Links.Get_Progress_Check);
                HttpContext.Session.SetString("Get_BulkCheck_Results", result.Links.Get_BulkCheck_Results);
                return RedirectToAction("Batch_Loader");
            }
        }

        public async Task<IActionResult> Batch_Loader()
        {
            
            var result = await _adminService.GetBulkCheckProgress(HttpContext.Session.GetString("Get_Progress_Check"));
            if (result != null)
            {
                TempData["totalCounter"] = result.Data.Total;
                TempData["currentCounter"] = result.Data.Complete;
                if (result.Data.Complete >= result.Data.Total)
                {
                    return RedirectToAction("Batch_check_success");
                }
            }
            
            return View();
        }

        public async Task<IActionResult> Batch_check_success()
        {
            return View("BatchOutcome/Success");
        }

        public async Task<IActionResult> Batch_check_download()
        {
            var resultData = await _adminService.GetBulkCheckResults(HttpContext.Session.GetString("Get_BulkCheck_Results"));
            var exportData = resultData.Data.Select(x=> new BatchFSMExport {
                LastName = x.LastName,
                DOB =x.DateOfBirth,
                NI = x.NationalInsuranceNumber,
                NASS = x.NationalAsylumSeekerServiceNumber,
                Outcome = x.Status.GetFsmStatusDescription()
            });

            var fileName = $"free-school-meal-outcomes-{DateTime.Now.ToString("yyyyMMdd")}.csv";

            var result = WriteCsvToMemory(exportData);
            var memoryStream = new MemoryStream(result);
            return new FileStreamResult(memoryStream, "text/csv") { FileDownloadName = fileName };
        }

        public byte[] WriteCsvToMemory(IEnumerable<BatchFSMExport> records)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter,CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(records);
                streamWriter.Flush();
                return memoryStream.ToArray();
            }
        }

        public IActionResult Process_Appeals()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }


        private int checkIfExists(int sequence, StringBuilder validationResultsItems, ValidationResult validationResults, int errorCount)
        {
            var message = "";
            if (errorCount >= TotalErrorsToDisplay)
            {
                errorCount++;
                return errorCount;
            }

            foreach (var item in validationResults.Errors)
            {

                switch (item.ErrorMessage)
                {
                    case CheckYourEligibility.Domain.Constants.ErrorMessages.FSM.LastName:
                    case "'LastName' must not be empty.":
                        {
                            message = $"<li>Line {sequence}: Issue with Surname</li>";
                            errorCount = AddLineIfNotExist(validationResultsItems, errorCount, message);
                        }
                        break;
                    case CheckYourEligibility.Domain.Constants.ErrorMessages.FSM.DOB
                    :
                    case "'Date Of Birth' must not be empty.":
                        {
                            message = $"<li>Line {sequence}: Issue with date of birth</li>";
                            errorCount = AddLineIfNotExist(validationResultsItems, errorCount, message);
                        }
                        break;
                    case CheckYourEligibility.Domain.Constants.ErrorMessages.FSM.NI:
                        {
                            message = $"<li>Line {sequence}: Issue with National Insurance number</li>";
                            errorCount = AddLineIfNotExist(validationResultsItems, errorCount, message);
                        }
                        break;
                    case CheckYourEligibility.Domain.Constants.ErrorMessages.FSM.NI_and_NASS:
                        {
                            message = $"<li>Line {sequence}: Issue {CheckYourEligibility.Domain.Constants.ErrorMessages.FSM.NI_and_NASS}</li>";
                            errorCount = AddLineIfNotExist(validationResultsItems, errorCount, message);
                        }
                        break;
                    case CheckYourEligibility.Domain.Constants.ErrorMessages.FSM.NI_or_NASS:
                        {
                            message = $"<li>Line {sequence}: Issue {CheckYourEligibility.Domain.Constants.ErrorMessages.FSM.NI_or_NASS}</li>";
                            errorCount = AddLineIfNotExist(validationResultsItems, errorCount, message);
                        }
                        break;
                    default:
                        message = $"<li>Line {sequence}: Issue {item.ErrorMessage}</li>";
                        if (!validationResultsItems.ToString().Contains(message))
                        {
                            validationResultsItems.AppendLine(message);
                            errorCount++;
                        }
                        break;
                }

            }
            return errorCount;
        }

        private static int AddLineIfNotExist(StringBuilder validationResultsItems, int errorCount, string message)
        {
            if (!validationResultsItems.ToString().Contains(message))
            {
                validationResultsItems.AppendLine(message);
                errorCount++;
            }

            return errorCount;
        }

        private static string GetStatusDescription(string status)
        {
            Enum.TryParse(status, out CheckEligibilityStatus statusEnum);

            switch (statusEnum)
            {
                case CheckEligibilityStatus.parentNotFound:
                    return "May not be entitled";
                case CheckEligibilityStatus.eligible:
                    return "Entitled";
                case CheckEligibilityStatus.notEligible:
                    return "Not Entitled";
                case CheckEligibilityStatus.DwpError:
                    return "Error";
                default:
                    return status.ToString();
            }
        }




    }
}