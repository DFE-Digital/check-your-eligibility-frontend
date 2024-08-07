using Azure.Core;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Mvc;

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
        
        public IActionResult Finalise()
        {
            return View();
        }
        public async Task<IActionResult> Process_Appeals()
        {
            _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);

            ApplicationRequestSearch applicationSearch = new ApplicationRequestSearch()
            {
                Data = new ApplicationRequestSearchData
                {

                    localAuthority = _Claims.Organisation.Category.Name == Constants.CategoryTypeLA ? Convert.ToInt32(_Claims.Organisation.Urn) : null,
                    School = _Claims.Organisation.Category.Name == Constants.CategoryTypeSchool ? Convert.ToInt32(_Claims.Organisation.Urn) : null,
                    Status = CheckYourEligibility.Domain.Enums.ApplicationStatus.EvidenceNeeded
                }
            };
            var resultsEvidenceNeeded = await _adminService.PostApplicationSearch(applicationSearch);
            resultsEvidenceNeeded ??= new ApplicationSearchResponse() { Data = new List<ApplicationResponse>() };
            applicationSearch.Data.Status = CheckYourEligibility.Domain.Enums.ApplicationStatus.SentForReview;
            var resultsSentForReview = await _adminService.PostApplicationSearch(applicationSearch);
            resultsSentForReview ??= new ApplicationSearchResponse() { Data = new List<ApplicationResponse>()};

            var resultItems = resultsEvidenceNeeded.Data.Union(resultsSentForReview.Data);
            var results = new ApplicationSearchResponse() { Data = resultItems };
            return View(results);
        }

        [HttpGet]
        public IActionResult Search()
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Results(ApplicationSearch request)
        {
            _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);

            ApplicationRequestSearch applicationSearch = new ApplicationRequestSearch()
            {
                Data = new ApplicationRequestSearchData
                {

                    localAuthority = _Claims.Organisation.Category.Name == Constants.CategoryTypeLA ? Convert.ToInt32(_Claims.Organisation.Urn) : null,
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

            return View(response);
        }
        [HttpGet]
        public async Task<IActionResult> ApplicationDetailAppeal(string id)
        {
                var response = await _adminService.GetApplication(id);

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
    } 
}
