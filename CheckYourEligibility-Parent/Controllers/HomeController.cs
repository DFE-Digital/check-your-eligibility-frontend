using CheckYourEligibility_FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Diagnostics;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Server.IIS;
using CheckYourEligibility_FrontEnd.ViewModels;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<CheckController> _logger;
        private readonly IEcsServiceParent _parentService;
        private readonly IEcsCheckService _checkService;
        private readonly IConfiguration _config;
        private IEcsServiceParent _object;

        public HomeController(ILogger<CheckController> logger, IEcsServiceParent ecsParentService, IEcsCheckService ecsCheckService, IConfiguration configuration)
        {
            _config = configuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentService = ecsParentService ?? throw new ArgumentNullException(nameof(ecsParentService));
            _checkService = ecsCheckService ?? throw new ArgumentNullException(nameof(ecsCheckService));

            _logger.LogInformation("controller log info");
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Accessibility()
        {
            return View();
        }

        public IActionResult Cookies()
        {
            return View();
        }

        public IActionResult fsm_print_version()
        {
            return View();
        }

        public IActionResult Parental_Guidance()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SchoolList()
        {
            var schoolList = await _parentService.GetSchool("school");

            // Check if schoolList or schoolList.Data is null
            var schools = schoolList?.Data?.ToList() ?? new List<CheckYourEligibility.Domain.Responses.Establishment>();

            var viewModel = new SchoolListViewModel
            {
                Schools = schools
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SchoolList(SchoolListViewModel viewModel)
        {
            if (viewModel.IsRadioSelected.HasValue)
            {
                if (viewModel.IsRadioSelected == true)
                {
                    return RedirectToAction("Enter_Details", "Check");
                }
                else
                {
                    return Redirect("https://www.gov.uk/apply-free-school-meals"); // this could be in appsettings instead
                }
            }
            else
            {
                var schoolList = await _parentService.GetSchool("school");
                var schools = schoolList?.Data?.ToList() ?? new List<CheckYourEligibility.Domain.Responses.Establishment>();

                viewModel.Schools = schools;
                return View(viewModel);
            }
        }


    }
}
