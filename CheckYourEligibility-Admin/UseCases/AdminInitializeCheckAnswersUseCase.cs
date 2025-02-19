using CheckYourEligibility_FrontEnd.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminInitializeCheckAnswersUseCase
    {
        Task<FsmApplication> Execute(string applicationJson);
    }

    public class AdminInitializeCheckAnswersUseCase : IAdminInitializeCheckAnswersUseCase
    {
        private readonly ILogger<AdminInitializeCheckAnswersUseCase> _logger;

        public AdminInitializeCheckAnswersUseCase(ILogger<AdminInitializeCheckAnswersUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<FsmApplication> Execute(string applicationJson)
        {
            try
            {
                if (string.IsNullOrEmpty(applicationJson))
                {
                    _logger.LogInformation("No FSM application data found in TempData");
                    return Task.FromResult<FsmApplication>(null);
                }

                FsmApplication application;
                try
                {
                    application = JsonConvert.DeserializeObject<FsmApplication>(applicationJson);
                }
                catch (JsonReaderException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize FSM application data");
                    return Task.FromResult<FsmApplication>(null);
                }

                if (application == null)
                {
                    _logger.LogWarning("Failed to deserialize FSM application data");
                    return Task.FromResult<FsmApplication>(null);
                }

                _logger.LogInformation("Successfully initialized Check_Answers view for parent: {ParentName}",
                    $"{application.ParentFirstName} {application.ParentLastName}");

                return Task.FromResult(application);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Check_Answers view");
                throw;
            }
        }
    }
}