using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }
    }

    public interface IAdminValidateParentDetailsUseCase
    {
        ValidationResult Execute(ParentGuardian request, ModelStateDictionary modelState);
    }

    public class AdminValidateParentDetailsUseCase : IAdminValidateParentDetailsUseCase
    {
        private readonly ILogger<AdminValidateParentDetailsUseCase> _logger;

        public AdminValidateParentDetailsUseCase(ILogger<AdminValidateParentDetailsUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ValidationResult Execute(ParentGuardian request, ModelStateDictionary modelState)
        {
            if (request.NinAsrSelection == ParentGuardian.NinAsrSelect.None)
            {
                if (!modelState.IsValid)
                {
                    var errors = ProcessModelStateErrors(modelState);
                    return new ValidationResult { IsValid = false, Errors = errors };
                }
            }
            else if (request.NinAsrSelection == ParentGuardian.NinAsrSelect.AsrnSelected)
            {
                modelState.Remove("NationalInsuranceNumber");
                if (!modelState.IsValid)
                {
                    var errors = ProcessModelStateErrors(modelState);
                    return new ValidationResult { IsValid = false, Errors = errors };
                }
            }
            else
            {
                modelState.Remove("NationalAsylumSeekerServiceNumber");
                if (!modelState.IsValid)
                {
                    var errors = ProcessModelStateErrors(modelState);
                    return new ValidationResult { IsValid = false, Errors = errors };
                }
            }

            return new ValidationResult { IsValid = true };
        }

        private Dictionary<string, List<string>> ProcessModelStateErrors(ModelStateDictionary modelState)
        {
            var errors = modelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    k => k.Key,
                    v => v.Value.Errors.Select(e => e.ErrorMessage).ToList()
                );

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

            return errors;
        }
    }
}