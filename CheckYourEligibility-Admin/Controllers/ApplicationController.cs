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

        public  Task<IActionResult> Search(ApplicationSearch request )
       {
            //Fetch result and pass it to view
            var applicationSearch = new ApplicationSearch
            {
                LocalAuthority = request.LocalAuthority,
                School = request.School,
                Status = request.Status,
                ChildLastName = request.ChildLastName,
                ParentLastName = request.ParentLastName,
                ReferenceNumber = request.ReferenceNumber,
                ChildDateOfBirth = new DateOnly(request.ChildDOBYear.Value, request.ChildDOBMonth.Value, request.ChildDOBDay.Value).ToString("yyyy-MM-dd"),
                ParentDateOfBirth = new DateOnly(request.PGDOBYear.Value, request.PGDOBMonth.Value, request.PGDOBDay.Value).ToString("yyyy-MM-dd")
            };
            Console.WriteLine(applicationSearch.ChildLastName);
            return null;
       }
    }
}
