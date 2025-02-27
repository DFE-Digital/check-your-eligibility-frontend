using System.Text;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Http;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface IPerformEligibilityCheckUseCase
    {
        Task<CheckEligibilityResponse> Execute(
            ParentGuardian parentRequest,
            ISession session
        );
    }

    public class PerformEligibilityCheckUseCase : IPerformEligibilityCheckUseCase
    {
        private readonly IEcsCheckService _checkService;

        public PerformEligibilityCheckUseCase(IEcsCheckService checkService)
        {
            _checkService = checkService ?? throw new ArgumentNullException(nameof(checkService));
        }

        public async Task<CheckEligibilityResponse> Execute(
            ParentGuardian parentRequest,
            ISession session)
        {
            session.Set("ParentFirstName", Encoding.UTF8.GetBytes(parentRequest.FirstName ?? string.Empty));
            session.Set("ParentLastName", Encoding.UTF8.GetBytes(parentRequest.LastName ?? string.Empty));

            // Build DOB string
            var dobString = new DateOnly(
                int.Parse(parentRequest.Year),
                int.Parse(parentRequest.Month),
                int.Parse(parentRequest.Day)
            ).ToString("yyyy-MM-dd");

            session.Set("ParentDOB", Encoding.UTF8.GetBytes(dobString));
            session.SetString("ParentEmail", parentRequest.EmailAddress);

            // If we're finishing a NASS flow, store "ParentNASS"; 
            // otherwise store "ParentNINO".
            if (parentRequest.NinAsrSelection == ParentGuardian.NinAsrSelect.AsrnSelected)
            {
                session.Set("ParentNASS", Encoding.UTF8.GetBytes(parentRequest.NationalAsylumSeekerServiceNumber ?? ""));
                session.Remove("ParentNINO");
            }
            else
            {
                session.Set("ParentNINO", Encoding.UTF8.GetBytes(parentRequest.NationalInsuranceNumber ?? ""));
                session.Remove("ParentNASS");
            }

            // Build ECS request
            var checkEligibilityRequest = new CheckEligibilityRequest_Fsm
            {
                Data = new CheckEligibilityRequestData_Fsm
                {
                    LastName = parentRequest.LastName,
                    NationalInsuranceNumber = parentRequest.NationalInsuranceNumber?.ToUpper(),
                    NationalAsylumSeekerServiceNumber = parentRequest.NationalAsylumSeekerServiceNumber?.ToUpper(),
                    DateOfBirth = dobString
                }
            };

            // Call ECS check
            
            
            var response = await _checkService.PostCheck(checkEligibilityRequest);

            return response;
        }
    }
}
