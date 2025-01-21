using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface IProcessParentDetailsUseCase
    {
        Task<(bool IsValid, CheckEligibilityResponse Response, string RedirectAction)> ExecuteAsync(
            Parent parentRequest,
            ISession session
        );
    }

    public class ProcessParentDetailsUseCase : IProcessParentDetailsUseCase
    {
        private readonly IEcsCheckService _checkService;

        public ProcessParentDetailsUseCase(IEcsCheckService checkService)
        {
            _checkService = checkService ?? throw new ArgumentNullException(nameof(checkService));
        }

        public async Task<(bool IsValid, CheckEligibilityResponse Response, string RedirectAction)> ExecuteAsync(
            Parent parentRequest,
            ISession session)
        {
            // Validate NINO and NASS logic
            if (parentRequest.IsNinoSelected == false && !parentRequest.NASSRedirect)
            {
                parentRequest.NASSRedirect = true;
                parentRequest.NationalInsuranceNumber = null;
                return (false, null, "Nass");
            }

            // Save session details
            session.SetString("ParentFirstName", parentRequest.FirstName);
            session.SetString("ParentLastName", parentRequest.LastName);
            session.SetString(
                "ParentDOB",
                new DateOnly(int.Parse(parentRequest.Year), int.Parse(parentRequest.Month), int.Parse(parentRequest.Day))
                    .ToString("yyyy-MM-dd")
            );

            if (!string.IsNullOrEmpty(parentRequest.NationalInsuranceNumber))
            {
                session.SetString("ParentNINO", parentRequest.NationalInsuranceNumber);
                session.Remove("ParentNASS");
            }

            if (!string.IsNullOrEmpty(parentRequest.NationalAsylumSeekerServiceNumber))
            {
                session.SetString("ParentNASS", parentRequest.NationalAsylumSeekerServiceNumber);
                session.Remove("ParentNINO");
            }

            // Build request for the API
            var checkEligibilityRequest = new CheckEligibilityRequest_Fsm
            {
                Data = new CheckEligibilityRequestData_Fsm
                {
                    LastName = parentRequest.LastName,
                    NationalInsuranceNumber = parentRequest.NationalInsuranceNumber?.ToUpper(),
                    NationalAsylumSeekerServiceNumber = parentRequest.NationalAsylumSeekerServiceNumber?.ToUpper(),
                    DateOfBirth = new DateOnly(int.Parse(parentRequest.Year), int.Parse(parentRequest.Month), int.Parse(parentRequest.Day))
                        .ToString("yyyy-MM-dd")
                }
            };

            // Call the API
            var response = await _checkService.PostCheck(checkEligibilityRequest);

            return (true, response, "Loader");
        }
    }
}
