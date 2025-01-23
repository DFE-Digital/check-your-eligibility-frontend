using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface ILoaderUseCase
    {
        Task<(string ViewName, object Model)> ExecuteAsync(string responseJson, ISession session);
    }

    public class LoaderUseCase : ILoaderUseCase
    {
        private readonly ILogger<LoaderUseCase> _logger;
        private readonly IEcsCheckService _checkService;

        public LoaderUseCase(
            ILogger<LoaderUseCase> logger,
            IEcsCheckService checkService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _checkService = checkService ?? throw new ArgumentNullException(nameof(checkService));
        }

        public async Task<(string ViewName, object Model)> ExecuteAsync(string responseJson, ISession session)
        {
            try
            {
                if (string.IsNullOrEmpty(responseJson))
                {
                    _logger.LogWarning("No response data found in TempData.");
                    return ("Outcome/Technical_Error", null);
                }

                var response = JsonConvert.DeserializeObject<CheckEligibilityResponse>(responseJson);
                _logger.LogInformation($"Check status processed: {response.Data.Status}");
                var check = await _checkService.GetStatus(response);

                if (check?.Data == null)
                {
                    _logger.LogWarning("Null response received from GetStatus.");
                    return ("Outcome/Technical_Error", null);
                }

                _logger.LogInformation($"Received status: {check.Data.Status}");
                session.SetString("CheckResult", check.Data.Status);

                return check.Data.Status switch
                {
                    "eligible" => ("Outcome/Eligible", "/check/signIn"),
                    "notEligible" => ("Outcome/Not_Eligible", null),
                    "parentNotFound" => ("Outcome/Not_Found", null),
                    "DwpError" => ("Outcome/Technical_Error", null),
                    "queuedForProcessing" => ("Loader", null),
                    _ => ("Outcome/Technical_Error", null) 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing eligibility check status");
                return ("Outcome/Technical_Error", null);
            }
        }
    }
}
