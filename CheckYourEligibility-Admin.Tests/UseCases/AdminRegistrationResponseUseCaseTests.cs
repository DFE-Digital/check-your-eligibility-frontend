using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility_Admin.Tests.UseCases
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
        private readonly IEcsServiceAdmin _adminService;

        public AdminRegistrationResponseUseCase(
            ILogger<AdminRegistrationResponseUseCase> logger,
            IEcsServiceAdmin adminService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
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

                if (request.Children?.ChildList == null || !request.Children.ChildList.Any())
                {
                    _logger.LogWarning("No children found in application request");
                    throw new AdminRegistrationResponseException("No children found in application request");
                }

                var applicationSearchRequest = new ApplicationRequestSearch
                {
                    Data = new ApplicationRequestSearchData
                    {
                        ParentLastName = request.ParentLastName,
                        ParentDateOfBirth = request.ParentDateOfBirth
                        
                    }
                };

                var searchResponse = await _adminService.PostApplicationSearch(applicationSearchRequest);

                if (searchResponse?.Data == null)
                {
                    throw new AdminRegistrationResponseException("Failed to retrieve application details");
                }

                var confirmation = new ApplicationConfirmationEntitledViewModel
                {
                    ParentName = $"{request.ParentFirstName} {request.ParentLastName}",
                    Children = new List<ApplicationConfirmationEntitledChildViewModel>()
                };

                foreach (var child in request.Children.ChildList)
                {
                    var childApplication = searchResponse.Data.FirstOrDefault(a =>
                        a.ChildFirstName == child.FirstName &&
                        a.ChildLastName == child.LastName);

                    confirmation.Children.Add(new ApplicationConfirmationEntitledChildViewModel
                    {
                        ParentName = confirmation.ParentName,
                        ChildName = $"{child.FirstName} {child.LastName}",
                        Reference = childApplication?.Reference ?? "Pending"
                    });
                }

                _logger.LogInformation("Successfully created registration confirmation for {ParentName} with {ChildCount} children",
                    confirmation.ParentName,
                    confirmation.Children.Count);

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