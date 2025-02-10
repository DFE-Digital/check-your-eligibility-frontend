using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminLoadParentDetailsUseCase
    {
        Task<(ParentGuardian Parent, Dictionary<string, List<string>> ValidationErrors)> Execute(
            string parentDetailsJson = null,
            string validationErrorsJson = null
        );
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
            try
            {
                var parent = DeserializeParent(parentDetailsJson);
                var errors = DeserializeAndProcessErrors(validationErrorsJson);

                return (parent, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load parent details");
                throw new AdminLoadParentDetailsException($"Failed to load parent details: {ex.Message}");
            }
        }

        private ParentGuardian DeserializeParent(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<ParentGuardian>(json);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize parent details");
                return null;
            }
        }

        private Dictionary<string, List<string>> DeserializeAndProcessErrors(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                var errors = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);

                if (errors == null)
                {
                    return null;
                }

                ProcessNinoNassValidation(errors);

                return errors;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize validation errors");
                return null;
            }
        }

        private void ProcessNinoNassValidation(Dictionary<string, List<string>> errors)
        {
            const string targetValue = "Please select one option";

            if (errors.ContainsKey("NationalInsuranceNumber") &&
                errors.ContainsKey("NationalAsylumSeekerServiceNumber"))
            {
                if (errors["NationalInsuranceNumber"].Contains(targetValue) &&
                    errors["NationalAsylumSeekerServiceNumber"].Contains(targetValue))
                {
                    errors.Remove("NationalInsuranceNumber");
                    errors.Remove("NationalAsylumSeekerServiceNumber");
                    errors["NINAS"] = new List<string> { targetValue };
                }
            }
        }
    }
}