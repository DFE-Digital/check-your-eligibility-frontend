using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    public class ApplicationController : BaseController
    {
        private readonly ILogger<ApplicationController> _logger;
        private readonly IEcsServiceAdmin _service;
        private readonly IEcsServiceAdmin _object;

        public ApplicationController(ILogger<ApplicationController> logger, IEcsServiceAdmin ecsServiceAdmin)
        {
            
            _logger = logger;
            _service = ecsServiceAdmin ?? throw new ArgumentNullException(nameof(ecsServiceAdmin));
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Results(ApplicationSearch request)
        {
            ApplicationRequestSearch applicationSearch = new ApplicationRequestSearch()
            {
                Data = new ApplicationRequestSearchData
                {

                    //localAuthority = null,
                    //School = _Claims.Organisation.Uid,
                    //Status = request.Status,
                    ChildLastName = request.ChildLastName,
                    //ParentLastName = request.ParentLastName,
                    //ReferenceNumber = request.ReferenceNumber, currently not in the model
                    //ChildDateOfBirth = new DateOnly(request.ChildDOBYear.Value, request.ChildDOBMonth.Value, request.ChildDOBDay.Value).ToString("yyyy-MM-dd"),
                    //ParentDateOfBirth = new DateOnly(request.PGDOBYear.Value, request.PGDOBMonth.Value, request.PGDOBDay.Value).ToString("yyyy-MM-dd")
                }
            };
            var response = await _service.PostApplicationSearch(applicationSearch);

            //Fetch result and pass it to view
            return null;
        }
    } 
}
