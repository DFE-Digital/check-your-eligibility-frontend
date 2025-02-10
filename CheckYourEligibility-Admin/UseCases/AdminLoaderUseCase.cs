using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminLoaderUseCase
    {
        Task<(string ViewName, object Model)> Execute(string responseJson, IEnumerable<Claim> claims);
    }

    [Serializable]
    public class AdminLoaderException : Exception
    {
        public AdminLoaderException(string message) : base(message)
        {
        }
    }

    public class AdminLoaderUseCase : IAdminLoaderUseCase
    {
        private readonly ILogger<AdminLoaderUseCase> _logger;
        private readonly IEcsCheckService _checkService;

        public AdminLoaderUseCase(
            ILogger<AdminLoaderUseCase> logger,
            IEcsCheckService checkService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _checkService = checkService ?? throw new ArgumentNullException(nameof(checkService));
        }

        public async Task<(string ViewName, object Model)> Execute(string responseJson, IEnumerable<Claim> claims)
        {
            try
            {
                if (string.IsNullOrEmpty(responseJson))
                {
                    return ("Outcome/Technical_Error", null);
                }

                var response = JsonConvert.DeserializeObject<CheckEligibilityResponse>(responseJson);
                var check = await _checkService.GetStatus(response);

                if (check?.Data == null)
                {
                    return ("Outcome/Technical_Error", null);
                }

                var isLocalAuthority = claims
                    .First(c => c.Type == "organisation")
                    .Value.Contains("LocalAuthority");

                return GetViewForStatus(check.Data.Status, isLocalAuthority);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process eligibility check status");
                throw new AdminLoaderException($"Failed to process eligibility check status: {ex.Message}");
            }
        }

        private (string ViewName, object Model) GetViewForStatus(string status, bool isLocalAuthority)
        {
            return status switch
            {
                nameof(CheckEligibilityStatus.eligible) => (
                    isLocalAuthority ? "Outcome/Eligible_LA" : "Outcome/Eligible",
                    null),
                nameof(CheckEligibilityStatus.notEligible) => (
                    isLocalAuthority ? "Outcome/Not_Eligible_LA" : "Outcome/Not_Eligible",
                    null),
                nameof(CheckEligibilityStatus.parentNotFound) => (
                    "Outcome/Not_Found",
                    null),
                nameof(CheckEligibilityStatus.DwpError) => (
                    "Outcome/Technical_Error",
                    null),
                nameof(CheckEligibilityStatus.queuedForProcessing) => (
                    "Loader",
                    null),
                _ => ("Outcome/Technical_Error", null)
            };
        }
    }
}