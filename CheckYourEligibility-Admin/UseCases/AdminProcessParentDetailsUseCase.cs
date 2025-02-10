using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminProcessParentDetailsUseCase
    {
        Task<(CheckEligibilityResponse Response, string RedirectAction)> Execute(
            ParentGuardian request,
            ISession session);
    }

    [Serializable]
    public class AdminProcessParentDetailsException : Exception
    {
        public AdminProcessParentDetailsException(string message) : base(message)
        {
        }
    }

    [Serializable]
    public class AdminParentDetailsValidationException : Exception
    {
        public AdminParentDetailsValidationException(string message) : base(message)
        {
        }
    }

    public class AdminProcessParentDetailsUseCase : IAdminProcessParentDetailsUseCase
    {
        private readonly ILogger<AdminProcessParentDetailsUseCase> _logger;
        private readonly IEcsCheckService _checkService;

        public AdminProcessParentDetailsUseCase(
            ILogger<AdminProcessParentDetailsUseCase> logger,
            IEcsCheckService checkService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _checkService = checkService ?? throw new ArgumentNullException(nameof(checkService));
        }

        public async Task<(CheckEligibilityResponse Response, string RedirectAction)> Execute(
            ParentGuardian request,
            ISession session)
        {
            try
            {
                _logger.LogInformation("Processing parent details for {LastName}", request.LastName);

                if (request.NinAsrSelection == ParentGuardian.NinAsrSelect.None)
                {
                    throw new AdminProcessParentDetailsException(
                        JsonConvert.SerializeObject(BuildNINASValidationError()));
                }

                StoreParentDetails(session, request);

                var checkRequest = BuildEligibilityRequest(request);
                var response = await _checkService.PostCheck(checkRequest);

                _logger.LogInformation("Check processed with status: {Status}", response.Data.Status);

                return (response, "Loader");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process parent details");
                throw new AdminProcessParentDetailsException($"Failed to process eligibility check: {ex.Message}");
            }
        }

        private Dictionary<string, List<string>> BuildNINASValidationError()
        {
            return new Dictionary<string, List<string>>
            {
                { "NINAS", new List<string> { "Please select one option" } }
            };
        }

        private void StoreParentDetails(ISession session, ParentGuardian request)
        {
            var dob = new DateOnly(
                int.Parse(request.Year),
                int.Parse(request.Month),
                int.Parse(request.Day)).ToString("yyyy-MM-dd");

            session.SetString("ParentFirstName", request.FirstName);
            session.SetString("ParentLastName", request.LastName);
            session.SetString("ParentDOB", dob);
            session.SetString("ParentEmail", request.EmailAddress);

            if (request.NinAsrSelection == ParentGuardian.NinAsrSelect.NinSelected)
            {
                session.SetString("ParentNINO", request.NationalInsuranceNumber?.ToUpper());
                session.Remove("ParentNASS");
            }
            else
            {
                session.SetString("ParentNASS", request.NationalAsylumSeekerServiceNumber);
                session.Remove("ParentNINO");
            }
        }

        private CheckEligibilityRequest_Fsm BuildEligibilityRequest(ParentGuardian request)
        {
            return new CheckEligibilityRequest_Fsm
            {
                Data = new CheckEligibilityRequestData_Fsm
                {
                    LastName = request.LastName,
                    NationalInsuranceNumber = request.NinAsrSelection == ParentGuardian.NinAsrSelect.NinSelected ?
                        request.NationalInsuranceNumber?.ToUpper() : null,
                    NationalAsylumSeekerServiceNumber = request.NinAsrSelection == ParentGuardian.NinAsrSelect.AsrnSelected ?
                        request.NationalAsylumSeekerServiceNumber : null,
                    DateOfBirth = new DateOnly(
                        int.Parse(request.Year),
                        int.Parse(request.Month),
                        int.Parse(request.Day)).ToString("yyyy-MM-dd")
                }
            };
        }
    }
}