using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.Security.Claims;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    public class ApplicationController : BaseController
    {
        private readonly ILogger<ApplicationController> _logger;

        public ApplicationController(ILogger<ApplicationController> logger)
        {
            
            _logger = logger;
            
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Search(ApplicationSearch request )
       {
            //Fetch result and pass it to view
            var applicationSearch = new ApplicationSearch
            {
                ChildName = request.ChildName,
                PGName = request.PGName,
                ReferenceNumber = request.ReferenceNumber,
                Status = request.Status,
                ChildDateOfBirth = new DateOnly(request.ChildDOBYear.Value, request.ChildDOBMonth.Value, request.ChildDOBDay.Value).ToString("yyyy-MM-dd"),
                ParentOrGuardianDateOfBirth = new DateOnly(request.PGDOBYear.Value, request.PGDOBMonth.Value, request.PGDOBDay.Value).ToString("yyyy-MM-dd")
            }
       }
    }
}
