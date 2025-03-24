using CheckYourEligibility.FrontEnd.Models;
using Newtonsoft.Json;

namespace CheckYourEligibility.FrontEnd.UseCases
{
    public class LoadParentDetailsViewModel
    {
        public Parent Parent { get; set; }
        public Dictionary<string, List<string>> ValidationErrors { get; set; }
    }

    public interface ILoadParentDetailsUseCase
    {
        Task<LoadParentDetailsViewModel> Execute(
            string parentDetailsJson = null,
            string validationErrorsJson = null
        );
    }

    public class LoadParentDetailsUseCase : ILoadParentDetailsUseCase
    {
        public async Task<LoadParentDetailsViewModel> Execute(
            string parentDetailsJson = null,
            string validationErrorsJson = null)
        {
            var viewModel = new LoadParentDetailsViewModel();

            if (!string.IsNullOrEmpty(parentDetailsJson))
            {
                try
                {
                    viewModel.Parent = JsonConvert.DeserializeObject<Parent>(parentDetailsJson);
                }
                catch (JsonException)
                {
                    // If deserialization fails, continue with null parent
                    viewModel.Parent = null;
                }
            }

            if (!string.IsNullOrEmpty(validationErrorsJson))
            {
                try
                {
                    var errors = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(
                        validationErrorsJson
                    );
                    errors?.Remove("NationalAsylumSeekerServiceNumber");
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