using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminSubmitApplicationUseCase
    {
        Task<(ApplicationConfirmationEntitledViewModel Result, ApplicationSaveItemResponse LastResponse)> Execute(
            FsmApplication request,
            string userId,
            string establishment);
    }

    public class AdminSubmitApplicationUseCase : IAdminSubmitApplicationUseCase
    {
        private readonly ILogger<AdminSubmitApplicationUseCase> _logger;
        private readonly IEcsServiceParent _parentService;

        public AdminSubmitApplicationUseCase(
            ILogger<AdminSubmitApplicationUseCase> logger,
            IEcsServiceParent parentService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentService = parentService ?? throw new ArgumentNullException(nameof(parentService));
        }

        public async Task<(ApplicationConfirmationEntitledViewModel Result, ApplicationSaveItemResponse LastResponse)> Execute(
    FsmApplication request,
    string userId,
    string establishment)
        {
            var parentName = $"{request.ParentFirstName} {request.ParentLastName}";
            var response = new ApplicationConfirmationEntitledViewModel
            {
                ParentName = parentName,
                Children = new List<ApplicationConfirmationEntitledChildViewModel>()
            };

            ApplicationSaveItemResponse lastResponse = null;

            foreach (var child in request.Children.ChildList)
            {
                var fsmApplication = new ApplicationRequest
                {
                    Data = new ApplicationRequestData
                    {
                        Type = CheckEligibilityType.FreeSchoolMeals,
                        ParentFirstName = request.ParentFirstName,
                        ParentLastName = request.ParentLastName,
                        ParentEmail = request.ParentEmail,
                        ParentDateOfBirth = request.ParentDateOfBirth,
                        ParentNationalInsuranceNumber = request.ParentNino,
                        ParentNationalAsylumSeekerServiceNumber = request.ParentNass,
                        ChildFirstName = child.FirstName,
                        ChildLastName = child.LastName,
                        ChildDateOfBirth = new DateOnly(
                            int.Parse(child.Year),
                            int.Parse(child.Month),
                            int.Parse(child.Day)).ToString("yyyy-MM-dd"),
                        Establishment = int.Parse(establishment),
                        UserId = userId
                    }
                };

                lastResponse = await _parentService.PostApplication_Fsm(fsmApplication);

                response.Children.Add(new ApplicationConfirmationEntitledChildViewModel
                {
                    ParentName = parentName,
                    ChildName = $"{lastResponse.Data.ChildFirstName} {lastResponse.Data.ChildLastName}",
                    Reference = lastResponse.Data.Reference
                });
            }

            return (response, lastResponse);
        }
    }
}
