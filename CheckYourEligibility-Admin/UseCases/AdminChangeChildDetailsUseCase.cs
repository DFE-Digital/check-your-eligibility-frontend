using CheckYourEligibility_FrontEnd.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminChangeChildDetailsUseCase
    {
        Children Execute(string fsmApplicationJson);
    }

    public class AdminChangeChildDetailsUseCase : IAdminChangeChildDetailsUseCase
    {
        private readonly ILogger<AdminChangeChildDetailsUseCase> _logger;

        public AdminChangeChildDetailsUseCase(ILogger<AdminChangeChildDetailsUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Children Execute(string fsmApplicationJson)
        {
            if (string.IsNullOrEmpty(fsmApplicationJson))
            {
                _logger.LogWarning("FSM application JSON was null or empty");
                return new Children { ChildList = new List<Child> { new Child() } };
            }

            var responses = JsonConvert.DeserializeObject<FsmApplication>(fsmApplicationJson);
            if (responses?.Children == null)
            {
                _logger.LogWarning("Deserialized FSM application or children was null");
                return new Children { ChildList = new List<Child> { new Child() } };
            }

            return responses.Children;
        }
    }
}
