using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd_Admin.Tests.UseCases
{
    public interface IAdminLoadParentDetailsUseCase
    {
        Task<(ParentGuardian Parent, Dictionary<string, List<string>> ValidationErrors)> Execute(
            string parentDetailsJson = null,
            string validationErrorsJson = null);
    }

    [Serializable]
    public class AdminLoadParentDetailsException : Exception
    {
        public AdminLoadParentDetailsException(string message) : base(message)
        {
        }
    }

    public class AdminLoadParentDetailsUseCase : IAdminLoadParentDetailsUseCase
    {
        private readonly ILogger<AdminLoadParentDetailsUseCase> _logger;

        public AdminLoadParentDetailsUseCase(ILogger<AdminLoadParentDetailsUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(ParentGuardian Parent, Dictionary<string, List<string>> ValidationErrors)> Execute(
            string parentDetailsJson = null,
            string validationErrorsJson = null)
        {
            ParentGuardian parent = null;
            Dictionary<string, List<string>> validationErrors = null;

            try
            {
                if (!string.IsNullOrEmpty(parentDetailsJson))
                {
                    parent = JsonConvert.DeserializeObject<ParentGuardian>(parentDetailsJson);
                }

                if (!string.IsNullOrEmpty(validationErrorsJson))
                {
                    validationErrors = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(validationErrorsJson);
                    ProcessSpecialCaseValidations(validationErrors);
                }

                return (parent, validationErrors);
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, "Failed to deserialize details");
                throw new AdminLoadParentDetailsException($"Failed to load parent details: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize parent details");
                return (null, null);
            }
        }

        private void ProcessSpecialCaseValidations(Dictionary<string, List<string>> errors)
        {
            if (errors == null) return;

            const string targetValue = "Please select one option";
            if (errors.TryGetValue("NationalInsuranceNumber", out var ninoErrors) &&
                errors.TryGetValue("NationalAsylumSeekerServiceNumber", out var nassErrors) &&
                ninoErrors.Contains(targetValue) && nassErrors.Contains(targetValue))
            {
                errors.Remove("NationalInsuranceNumber");
                errors.Remove("NationalAsylumSeekerServiceNumber");
                errors["NINAS"] = new List<string> { targetValue };
            }
        }
    }
}