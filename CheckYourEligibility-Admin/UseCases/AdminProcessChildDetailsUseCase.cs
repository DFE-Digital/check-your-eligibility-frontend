using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Http;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminProcessChildDetailsUseCase
    {
        Task<FsmApplication> Execute(Children request, bool isRedirect, ISession session);
    }

    [Serializable]
    public class AdminProcessChildDetailsException : Exception
    {
        public AdminProcessChildDetailsException(string message) : base(message)
        {
        }
    }

    public class AdminProcessChildDetailsUseCase : IAdminProcessChildDetailsUseCase
    {
        private readonly ILogger<AdminProcessChildDetailsUseCase> _logger;
        private readonly IEcsServiceParent _parentService;

        public AdminProcessChildDetailsUseCase(
            ILogger<AdminProcessChildDetailsUseCase> logger,
            IEcsServiceParent parentService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentService = parentService ?? throw new ArgumentNullException(nameof(parentService));
        }

        public async Task<FsmApplication> Execute(Children request, bool isRedirect, ISession session)
        {
            try
            {
                if (isRedirect)
                {
                    _logger.LogInformation("Processing child details redirect");
                    return new FsmApplication { Children = request };
                }

                _logger.LogInformation("Creating new FSM application with child details");
                return CreateFsmApplication(request, session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process child details");
                throw new AdminProcessChildDetailsException($"Failed to process child details: {ex.Message}");
            }
        }

        private FsmApplication CreateFsmApplication(Children request, ISession session)
        {
            return new FsmApplication
            {
                ParentFirstName = session.GetString("ParentFirstName"),
                ParentLastName = session.GetString("ParentLastName"),
                ParentDateOfBirth = session.GetString("ParentDOB"),
                ParentNass = session.GetString("ParentNASS"),
                ParentNino = session.GetString("ParentNINO"),
                ParentEmail = session.GetString("ParentEmail"),
                Children = request
            };
        }
    }
}