using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminRegistrationResponseUseCase
    {
        Task<ApplicationConfirmationEntitledViewModel> Execute(FsmApplication request);
    }

    [Serializable]
    public class AdminRegistrationResponseException : Exception
    {
        public AdminRegistrationResponseException(string message) : base(message)
        {
        }
    }

    public class AdminRegistrationResponseUseCase : IAdminRegistrationResponseUseCase
    {
        private readonly ILogger<AdminRegistrationResponseUseCase> _logger;

        public AdminRegistrationResponseUseCase(ILogger<AdminRegistrationResponseUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApplicationConfirmationEntitledViewModel> Execute(FsmApplication request)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogWarning("Application request is null");
                    throw new AdminRegistrationResponseException("Application request data not found");
                }

                // Create the confirmation view model
                var confirmation = new ApplicationConfirmationEntitledViewModel
                {
                    ParentName = $"{request.ParentFirstName} {request.ParentLastName}"
                    // Add other properties as needed
                };

                _logger.LogInformation("Successfully created registration confirmation for {ParentName}",
                    confirmation.ParentName);

                return confirmation;
            }
            catch (Exception ex) when (ex is not AdminRegistrationResponseException)
            {
                _logger.LogError(ex, "Failed to process registration response");
                throw new AdminRegistrationResponseException($"Failed to process registration response: {ex.Message}");
            }
        }
    }
}