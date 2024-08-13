// Ignore Spelling: Finalise

using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.ViewModels;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Reflection;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    public class ApplicationController : BaseController
    {
        private readonly ILogger<ApplicationController> _logger;
        private readonly IEcsServiceAdmin _adminService;

        public ApplicationController(ILogger<ApplicationController> logger, IEcsServiceAdmin ecsServiceAdmin)
        {
            
            _logger = logger;
            _adminService = ecsServiceAdmin ?? throw new ArgumentNullException(nameof(ecsServiceAdmin));
            

        }

        #region Search

        [HttpGet]
        public IActionResult Search()
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Results(ApplicationSearch request)
        {
            if (!ModelState.IsValid)
            {       
                TempData["ApplicationSearch"] = JsonConvert.SerializeObject(request);
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList());
                TempData["Errors"] = JsonConvert.SerializeObject(errors);
                return RedirectToAction("Search");
            }

            _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);

            ApplicationRequestSearch applicationSearch = new ApplicationRequestSearch()
            {
                Data = new ApplicationRequestSearchData
                {
                    localAuthority = _Claims.Organisation.Category.Name == Constants.CategoryTypeLA ? Convert.ToInt32(_Claims.Organisation.EstablishmentNumber) : null,
                    School = _Claims.Organisation.Category.Name == Constants.CategoryTypeSchool ? Convert.ToInt32(_Claims.Organisation.Urn) : null,
                    Status = request.Status,
                    ChildLastName = request.ChildLastName,
                    ParentLastName = request.ParentLastName,
                    Reference = request.Reference,
                    ChildDateOfBirth =  request.ChildDOBYear.HasValue ?
                    new DateOnly(request.ChildDOBYear.Value, request.ChildDOBMonth.Value, request.ChildDOBDay.Value).ToString("yyyy-MM-dd")
                    : null,
                    ParentDateOfBirth = request.PGDOBYear.HasValue ?
                    new DateOnly(request.PGDOBYear.Value, request.PGDOBMonth.Value, request.PGDOBDay.Value).ToString("yyyy-MM-dd")
                    : null,
                }
            };
            var response = await _adminService.PostApplicationSearch(applicationSearch);

            response ??= new ApplicationSearchResponse() { Data = new List<ApplicationResponse>()};

            if (response.Data == null || !response.Data.Any())
            {
                TempData["Message"] = "There are no records matching your search.";
                return RedirectToAction("Search");
            }

            return View(response);
        }

        [HttpGet]
        public async Task<IActionResult> ApplicationDetail(string id)
        {
            var response = await _adminService.GetApplication(id);
            if (response == null)
            {
                return NotFound();
            }
            if (!CheckAccess(response)){
                return new UnauthorizedResult();
            }
            
            return View(response);
        }

        #endregion

        #region Appeals

        public async Task<IActionResult> Process_Appeals()
        {
            _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);
            ApplicationRequestSearch applicationSearch = new ApplicationRequestSearch()
            {
                Data = new ApplicationRequestSearchData
                {

                    localAuthority = _Claims.Organisation.Category.Name == Constants.CategoryTypeLA ? Convert.ToInt32(_Claims.Organisation.EstablishmentNumber) : null,
                    School = _Claims.Organisation.Category.Name == Constants.CategoryTypeSchool ? Convert.ToInt32(_Claims.Organisation.Urn) : null,
                    Status = CheckYourEligibility.Domain.Enums.ApplicationStatus.EvidenceNeeded
                }
            };
            var resultsEvidenceNeeded = await _adminService.PostApplicationSearch(applicationSearch);
            resultsEvidenceNeeded ??= new ApplicationSearchResponse() { Data = new List<ApplicationResponse>() };
            applicationSearch.Data.Status = CheckYourEligibility.Domain.Enums.ApplicationStatus.SentForReview;
            var resultsSentForReview = await _adminService.PostApplicationSearch(applicationSearch);
            resultsSentForReview ??= new ApplicationSearchResponse { Data = new List<ApplicationResponse>() };

            var resultItems = resultsEvidenceNeeded.Data.Union(resultsSentForReview.Data);
            var results = new ApplicationSearchResponse() { Data = resultItems };
            return View(results);

        }

        [HttpGet]
        public IActionResult EvidenceGuidance()
        {

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ApplicationDetailAppeal(string id)
        {
            var response = await _adminService.GetApplication(id);
            if (response == null)
            {
                return NotFound();
            }
            if (!CheckAccess(response))
            {
                return new UnauthorizedResult();
            }

            return View(response);
        }


        [HttpGet]
        public async Task<IActionResult> ApplicationDetailAppealConfirmation(string id)
        {
            TempData["AppAppealID"] = id;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ApplicationDetailAppealSend(string id)
        {
            await _adminService.PatchApplicationStatus(id, CheckYourEligibility.Domain.Enums.ApplicationStatus.SentForReview);
            
            return RedirectToAction("Process_Appeals");
        }

        public IActionResult Finalise()
        {
            return View();
        }

        #endregion

        #region School finalise Applications

        public async Task<IActionResult> FinaliseApplications()
        {
            ApplicationSearchResponse results = await GetFinalisedApplications();

            var viewModel = results.Data.Select(x => new SelectPersonEditorViewModel { Person = x });
            var viewData = new PeopleSelectionViewModel { People = viewModel.ToList() };

            return View(viewData);

        }

        [HttpGet]
        public async Task<IActionResult> ApplicationDetailFinalise(string id)
        {
            var response = await _adminService.GetApplication(id);
            if (response == null)
            {
                return NotFound();
            }
            if (!CheckAccess(response))
            {
                return new UnauthorizedResult();
            }

            return View(response);
        }

        [HttpPost]
        public ActionResult FinaliseSelectedApplications(PeopleSelectionViewModel model)
        {
            
            TempData["FinaliseApplicationIds"] = model.getSelectedIds(); 

            return View("ApplicationFinaliseConfirmation");
        }


        [HttpGet]
        public async Task<IActionResult> ApplicationFinaliseSend()
        {
            foreach (var id in TempData["FinaliseApplicationIds"] as IEnumerable<string>)
            {
                await _adminService.PatchApplicationStatus(id, CheckYourEligibility.Domain.Enums.ApplicationStatus.Receiving);

            }
            return RedirectToAction("FinaliseApplications");
        }

        public async Task<IActionResult> FinalisedApplicationsdownload()
        {
            var resultData = await GetFinalisedApplications();

            var fileName = $"finalise-applications-{DateTime.Now.ToString("yyyyMMdd")}.csv";

            var result = WriteCsvToMemory(resultData.Data.Select(x=> new ApplicationExport {
                Reference= x.Reference,
                Parent = $"{x.ParentFirstName} {x.ParentLastName}",
                Child = $"{x.ChildFirstName} {x.ChildLastName}",
                ChildDOB = Convert.ToDateTime(x.ChildDateOfBirth).ToString("dd MMM yyyy"),
                Status = x.Status.GetFsmStatusDescription(),
                SubmisionDate = x.Created.ToString("dd MMM yyyy")

            }));
            var memoryStream = new MemoryStream(result);
            return new FileStreamResult(memoryStream, "text/csv") { FileDownloadName = fileName };
        }




        #endregion

        private byte[] WriteCsvToMemory(IEnumerable<ApplicationExport> records)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(records);
                streamWriter.Flush();
                return memoryStream.ToArray();
            }
        }

        private async Task<ApplicationSearchResponse> GetFinalisedApplications()
        {
            _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);
            ApplicationRequestSearch applicationSearch = new ApplicationRequestSearch()
            {
                Data = new ApplicationRequestSearchData
                {

                    localAuthority = _Claims.Organisation.Category.Name == Constants.CategoryTypeLA ? Convert.ToInt32(_Claims.Organisation.EstablishmentNumber) : null,
                    School = _Claims.Organisation.Category.Name == Constants.CategoryTypeSchool ? Convert.ToInt32(_Claims.Organisation.Urn) : null,
                    Status = CheckYourEligibility.Domain.Enums.ApplicationStatus.Entitled
                }
            };
            var resultsEvidenceNeeded = await _adminService.PostApplicationSearch(applicationSearch);
            resultsEvidenceNeeded ??= new ApplicationSearchResponse() { Data = new List<ApplicationResponse>() };
            applicationSearch.Data.Status = CheckYourEligibility.Domain.Enums.ApplicationStatus.ReviewedEntitled;
            var resultsSentForReview = await _adminService.PostApplicationSearch(applicationSearch);
            resultsSentForReview ??= new ApplicationSearchResponse { Data = new List<ApplicationResponse>() };
            var resultItems = resultsEvidenceNeeded.Data.Union(resultsSentForReview.Data);
            var results = new ApplicationSearchResponse() { Data = resultItems };
            return results;
        }


        private bool CheckAccess(ApplicationItemResponse response)
        {
            _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);
            if ((_Claims.Organisation.Category.Name == Constants.CategoryTypeSchool ? Convert.ToInt32(_Claims.Organisation.Urn) : null) != null)
            {
                if (response.Data.School.Id.ToString() != _Claims.Organisation.Urn)
                {
                    _logger.LogError($"Invalid School access attempt {response.Data.School.Id} organisation Urn:-{_Claims.Organisation.Urn}");
                    return false;
                }
            }
            if ((_Claims.Organisation.Category.Name == Constants.CategoryTypeLA ? Convert.ToInt32(_Claims.Organisation.Urn) : null) != null)
            {
                if (response.Data.School.LocalAuthority.Id.ToString() != _Claims.Organisation.Urn)
                {
                    _logger.LogError($"Invalid Local Authority access attempt {response.Data.School.LocalAuthority.Id} organisation Urn:-{_Claims.Organisation.Urn}");
                    return false;
                }
            }
            return true;
        }
    } 
}
