using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminRegistrationResponseUseCase
    {
        Task<ApplicationConfirmationEntitledViewModel> Execute(FsmApplication request);
    }

    public class AdminRegistrationResponseUseCase : IAdminRegistrationResponseUseCase
    {
        private readonly ILogger<AdminRegistrationResponseUseCase> _logger;

        public AdminRegistrationResponseUseCase(ILogger<AdminRegistrationResponseUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<ApplicationConfirmationEntitledViewModel> Execute(FsmApplication request)
        {
            var parentName = $"{request.ParentFirstName} {request.ParentLastName}";
            var response = new ApplicationConfirmationEntitledViewModel
            {
                ParentName = parentName,
                Children = new List<ApplicationConfirmationEntitledChildViewModel>()
            };

            foreach (var child in request.Children.ChildList)
            {
                response.Children.Add(new ApplicationConfirmationEntitledChildViewModel
                {
                    ParentName = parentName,
                    ChildName = $"{child.FirstName} {child.LastName}",
                    Reference = $"{DateTime.Now:yyyyMMddHHmmss}-{child.ChildIndex}"
                });
            }

            _logger.LogInformation("Created registration response for parent {ParentName}", parentName);

            return Task.FromResult(response);
        }
    }
}