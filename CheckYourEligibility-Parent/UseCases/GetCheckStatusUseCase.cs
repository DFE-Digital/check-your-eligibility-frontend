using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface IGetCheckStatusUseCase
    {
        Task<string> ExecuteAsync(string responseJson, ISession session);
    }

    public class GetCheckStatusUseCase : IGetCheckStatusUseCase
    {
        private readonly ILogger<GetCheckStatusUseCase> _logger;
        private readonly IEcsCheckService _checkService;

        public GetCheckStatusUseCase(
            ILogger<GetCheckStatusUseCase> logger,
            IEcsCheckService checkService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _checkService = checkService ?? throw new ArgumentNullException(nameof(checkService));
        }

        public async Task<string> ExecuteAsync(string responseJson, ISession session)
        {
            if (string.IsNullOrEmpty(responseJson))
            {
                _logger.LogWarning("No response data found in TempData.");
                throw new Exception("No response data found in TempData.");
            }

            var response = JsonConvert.DeserializeObject<CheckEligibilityResponse>(responseJson);
            _logger.LogInformation($"Check status processed: {response.Data.Status}");
            var check = await _checkService.GetStatus(response);

            if (check?.Data == null)
            {
                _logger.LogWarning("Null response received from GetStatus.");
                throw new Exception("Null response received from GetStatus.");
            }

            _logger.LogInformation($"Received status: {check.Data.Status}");
            session.SetString("CheckResult", check.Data.Status);

            return check.Data.Status;
        }
    }
}
