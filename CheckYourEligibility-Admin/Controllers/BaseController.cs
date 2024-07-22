using CheckYourEligibility_DfeSignIn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheckYourEligibility_FrontEnd.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {

        protected DfeClaims? _Claims;

    }
}
