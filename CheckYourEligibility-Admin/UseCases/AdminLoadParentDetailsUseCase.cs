using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public class AdminLoadParentDetailsViewModel
    {
        public ParentGuardian Parent { get; set; }
        public Dictionary<string, List<string>> ValidationErrors { get; set; }
    }

    public interface IAdminLoadParentDetailsUseCase
    {
        Task<AdminLoadParentDetailsViewModel> ExecuteAsync(
            string parentDetailsJson = null,
            string validationErrorsJson = null
        );
    }

    public class AdminLoadParentDetailsUseCase : IAdminLoadParentDetailsUseCase
    {
        public async Task<AdminLoadParentDetailsViewModel> ExecuteAsync(
            string parentDetailsJson = null,
            string validationErrorsJson = null)
        {
            var viewModel = new AdminLoadParentDetailsViewModel();

            if (!string.IsNullOrEmpty(parentDetailsJson))
            {
                try
                {
                    viewModel.Parent = JsonConvert.DeserializeObject<ParentGuardian>(parentDetailsJson);
                }
                catch (JsonException)
                {
                    viewModel.Parent = null;
                }
            }

            if (!string.IsNullOrEmpty(validationErrorsJson))
            {
                try
                {
                    var errors = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(validationErrorsJson);

                    // Handle special case for NIN/ASR validation
                    if (errors.ContainsKey("NationalInsuranceNumber") &&
                        errors.ContainsKey("NationalAsylumSeekerServiceNumber"))
                    {
                        string targetValue = "Please select one option";
                        if (errors["NationalInsuranceNumber"].Contains(targetValue) &&
                            errors["NationalAsylumSeekerServiceNumber"].Contains(targetValue))
                        {
                            errors.Remove("NationalInsuranceNumber");
                            errors.Remove("NationalAsylumSeekerServiceNumber");
                            errors["NINAS"] = new List<string> { targetValue };
                        }
                    }
                    viewModel.ValidationErrors = errors;
                }
                catch (JsonException)
                {
                    viewModel.ValidationErrors = null;
                }
            }

            return viewModel;
        }
    }
}