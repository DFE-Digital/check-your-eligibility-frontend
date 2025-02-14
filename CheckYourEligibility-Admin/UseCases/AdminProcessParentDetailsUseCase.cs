using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminProcessParentDetailsUseCase
    {
        Task<CheckEligibilityResponse> Execute(ParentGuardian request, ISession session);
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

        public async Task<CheckEligibilityResponse> Execute(ParentGuardian request, ISession session)
        {
            try
            {
                // Exactly match original date formatting
                var dateOfBirth = new DateOnly(
                    int.Parse(request.Year),
                    int.Parse(request.Month),
                    int.Parse(request.Day))
                    .ToString("yyyy-MM-dd");

                var checkEligibilityRequest = new CheckEligibilityRequest_Fsm
                {
                    Data = new CheckEligibilityRequestData_Fsm
                    {
                        LastName = request.LastName,
                        DateOfBirth = dateOfBirth
                    }
                };

                // Store details in session exactly as original
                session.SetString("ParentFirstName", request.FirstName);
                session.SetString("ParentLastName", request.LastName);
                session.SetString("ParentDOB", dateOfBirth);
                session.SetString("ParentEmail", request.EmailAddress);

                // Handle NASS vs NINO exactly as original
                if (request.NinAsrSelection == ParentGuardian.NinAsrSelect.AsrnSelected)
                {
                    checkEligibilityRequest.Data.NationalAsylumSeekerServiceNumber = request.NationalAsylumSeekerServiceNumber;
                    session.SetString("ParentNASS", request.NationalAsylumSeekerServiceNumber);
                    session.Remove("ParentNINO");
                }
                else
                {
                    checkEligibilityRequest.Data.NationalInsuranceNumber = request.NationalInsuranceNumber?.ToUpper();
                    session.SetString("ParentNINO", request.NationalInsuranceNumber);
                    session.Remove("ParentNASS");
                }

                var response = await _checkService.PostCheck(checkEligibilityRequest);

                _logger.LogInformation($"Check processed:- {response.Data.Status} {response.Links.Get_EligibilityCheck}");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing parent details");
                throw;
            }
        }
    }
}