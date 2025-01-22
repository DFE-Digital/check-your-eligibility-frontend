using CheckYourEligibility_FrontEnd.Models;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface IChangeChildDetailsUseCase
    {
        Task<(bool IsSuccess, string ViewName, Children Model)> ExecuteAsync(string fsmApplicationJson);
    }

    public class ChangeChildDetailsUseCase : IChangeChildDetailsUseCase
    {
        private readonly ILogger<ChangeChildDetailsUseCase> _logger;

        public ChangeChildDetailsUseCase(ILogger<ChangeChildDetailsUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<(bool IsSuccess, string ViewName, Children Model)> ExecuteAsync(string fsmApplicationJson)
        {
            try
            {
                if (string.IsNullOrEmpty(fsmApplicationJson))
                {
                    _logger.LogWarning("FSM Application JSON is null or empty");
                    return Task.FromResult((false, "Enter_Child_Details", new Children { ChildList = new List<Child>() }));
                }

                var application = JsonConvert.DeserializeObject<FsmApplication>(fsmApplicationJson);
                if (application?.Children == null)
                {
                    _logger.LogWarning("Failed to deserialize FSM Application or Children is null");
                    return Task.FromResult((false, "Enter_Child_Details", new Children { ChildList = new List<Child>() }));
                }

                _logger.LogInformation("Successfully retrieved children details for change");
                return Task.FromResult((true, "Enter_Child_Details", application.Children));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing change child details");
                throw;
            }
        }
    }
}