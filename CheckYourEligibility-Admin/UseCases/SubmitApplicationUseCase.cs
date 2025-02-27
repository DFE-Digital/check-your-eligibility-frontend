using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface ISubmitApplicationUseCase
    {
        Task<List<ApplicationSaveItemResponse>> Execute(
            FsmApplication request,
            string userId,
            string establishment);
    }

    public class SubmitApplicationUseCase : ISubmitApplicationUseCase
    {
        private readonly ILogger<SubmitApplicationUseCase> _logger;
        private readonly IEcsServiceParent _parentService;

        public SubmitApplicationUseCase(
            ILogger<SubmitApplicationUseCase> logger,
            IEcsServiceParent parentService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentService = parentService ?? throw new ArgumentNullException(nameof(parentService));
        }

        public async Task<List<ApplicationSaveItemResponse>> Execute(
            FsmApplication request,
            string userId,
            string establishment)
        {
            var responses = new List<ApplicationSaveItemResponse>();

            foreach (var child in request.Children.ChildList)
            {
                var application = new ApplicationRequest
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
                var response = await _parentService.PostApplication_Fsm(application);
                responses.Add(response);
            }

            return responses;
        }
    }
}
