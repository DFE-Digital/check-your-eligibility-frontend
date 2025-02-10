using CheckYourEligibility_FrontEnd.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminChangeChildDetailsUseCase
    {
        Task<Children> Execute(string fsmApplicationJson);
    }

    [Serializable]
    public class AdminChangeChildDetailsException : Exception
    {
        public AdminChangeChildDetailsException(string message) : base(message) { }
    }

    public class AdminChangeChildDetailsUseCase : IAdminChangeChildDetailsUseCase
    {
        private readonly ILogger<AdminChangeChildDetailsUseCase> _logger;

        public AdminChangeChildDetailsUseCase(ILogger<AdminChangeChildDetailsUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Children> Execute(string fsmApplicationJson)
        {
            try
            {
                if (string.IsNullOrEmpty(fsmApplicationJson))
                {
                    throw new AdminChangeChildDetailsException("FSM Application JSON is null or empty");
                }

                try
                {
                    var application = JsonConvert.DeserializeObject<FsmApplication>(fsmApplicationJson);
                    if (application?.Children == null)
                    {
                        throw new AdminChangeChildDetailsException("Failed to deserialize FSM Application or Children is null");
                    }

                    _logger.LogInformation("Successfully loaded children details for change request");
                    return application.Children;
                }
                catch (JsonException)
                {
                    throw new AdminChangeChildDetailsException("Failed to parse application data");
                }
            }
            catch (AdminChangeChildDetailsException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while changing child details");
                throw new AdminChangeChildDetailsException($"Failed to change child details: {ex.Message}");
            }
        }
    }
}