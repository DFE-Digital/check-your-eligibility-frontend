using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
   
    public class AdminApplicationsRegisteredResult
    {
        public bool IsSuccess { get; set; }
        public ApplicationConfirmationEntitledViewModel? ViewModel { get; set; }
        public string? ErrorViewName { get; set; }

        public static AdminApplicationsRegisteredResult Success(ApplicationConfirmationEntitledViewModel viewModel) =>
            new() { IsSuccess = true, ViewModel = viewModel };

        public static AdminApplicationsRegisteredResult Error(string errorViewName) =>
            new() { IsSuccess = false, ErrorViewName = errorViewName };
    }

    public interface IAdminApplicationsRegisteredUseCase
    {
        /// <summary>
        /// Processes the admin applications registered request by validating and deserializing the stored application data.
        /// </summary>
        /// <param name="applicationJson">The JSON string containing the application data.</param>
        /// <returns>An AdminApplicationsRegisteredResult indicating success or failure with appropriate data.</returns>
        Task<AdminApplicationsRegisteredResult> Execute(string? applicationJson);
    }

    public class AdminApplicationsRegisteredUseCase : IAdminApplicationsRegisteredUseCase
    {
        private readonly ILogger<AdminApplicationsRegisteredUseCase> _logger;

        public AdminApplicationsRegisteredUseCase(ILogger<AdminApplicationsRegisteredUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AdminApplicationsRegisteredResult> Execute(string? applicationJson)
        {
            try
            {
                await Task.CompletedTask; 

                _logger.LogInformation("Processing admin applications registered request with JSON: {json}", applicationJson);

                if (string.IsNullOrEmpty(applicationJson))
                {
                    _logger.LogWarning("Application JSON is null or empty");
                    return AdminApplicationsRegisteredResult.Error("Outcome/Technical_Error");
                }

                var viewModel = JsonConvert.DeserializeObject<ApplicationConfirmationEntitledViewModel>(applicationJson);
                if (viewModel == null)
                {
                    _logger.LogWarning("Deserialization returned null for application");
                    return AdminApplicationsRegisteredResult.Error("Outcome/Technical_Error");
                }

                // Validate and initialize required properties
                if (string.IsNullOrEmpty(viewModel.ParentName))
                {
                    _logger.LogWarning("viewModel.ParentName is null or empty");
                }

                if (viewModel.Children == null)
                {
                    _logger.LogWarning("viewModel.Children is null. Initializing to an empty list");
                    viewModel.Children = new List<ApplicationConfirmationEntitledChildViewModel>();
                }

                _logger.LogInformation("Successfully processed admin application for parent: {parentName}",
                    viewModel.ParentName);

                return AdminApplicationsRegisteredResult.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing admin applications registered request");
                return AdminApplicationsRegisteredResult.Error("Outcome/Technical_Error");
            }
        }
    }
}