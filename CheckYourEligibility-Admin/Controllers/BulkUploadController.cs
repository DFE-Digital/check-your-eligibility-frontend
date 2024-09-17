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
    public class BulkUploadController : BaseController
    {
        const int TotalErrorsToDisplay = 20;

        private readonly ILogger<BulkUploadController> _logger;
        private readonly IEcsCheckService _checkService;
        private readonly IConfiguration _config;
        private ILogger<BulkUploadController> _loggerMock;
                
        public BulkUploadController(ILogger<BulkUploadController> logger, IEcsCheckService ecsCheckService, IConfiguration configuration)
        {
            _config = configuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _checkService = ecsCheckService ?? throw new ArgumentNullException(nameof(ecsCheckService));
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
                TempData["ErrorMessage"] = "Select a CSV File";
                return RedirectToAction("Batch_Check");
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
                        Sequence = sequence,
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
                var result = await _checkService.PostBulkCheck(new CheckEligibilityRequestBulk { Data = requestItems});
                HttpContext.Session.SetString("Get_Progress_Check", result.Links.Get_Progress_Check);
                HttpContext.Session.SetString("Get_BulkCheck_Results", result.Links.Get_BulkCheck_Results);
                return RedirectToAction("Batch_Loader");
            }
        }

        public async Task<IActionResult> Batch_Loader()
        {
            
            var result = await _checkService.GetBulkCheckProgress(HttpContext.Session.GetString("Get_Progress_Check"));
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
            var resultData = await _checkService.GetBulkCheckResults(HttpContext.Session.GetString("Get_BulkCheck_Results"));
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

        private byte[] WriteCsvToMemory(IEnumerable<BatchFSMExport> records)
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
    }
}