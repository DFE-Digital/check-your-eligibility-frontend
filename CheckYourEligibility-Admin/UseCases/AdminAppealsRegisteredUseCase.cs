using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public class AdminAppealsRegisteredResult
    {
        public bool IsSuccess { get; set; }
        public ApplicationConfirmationEntitledViewModel? ViewModel { get; set; }
        public string? ErrorViewName { get; set; }

        public static AdminAppealsRegisteredResult Success(ApplicationConfirmationEntitledViewModel viewModel) =>
            new() { IsSuccess = true, ViewModel = viewModel };

        public static AdminAppealsRegisteredResult Error(string errorViewName) =>
            new() { IsSuccess = false, ErrorViewName = errorViewName };
    }

    public interface IAdminAppealsRegisteredUseCase
    {
        Task<AdminAppealsRegisteredResult> Execute(string? applicationJson);
    }

    public class AdminAppealsRegisteredUseCase : IAdminAppealsRegisteredUseCase
    {
        private readonly ILogger<AdminAppealsRegisteredUseCase> _logger;

        public AdminAppealsRegisteredUseCase(ILogger<AdminAppealsRegisteredUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AdminAppealsRegisteredResult> Execute(string? applicationJson)
        {
            try
            {
                await Task.CompletedTask; // Ensure async context

                _logger.LogInformation("Processing admin appeals registration with JSON: {json}", applicationJson);

                if (string.IsNullOrEmpty(applicationJson))
                {
                    _logger.LogWarning("Application JSON is null or empty");
                    return AdminAppealsRegisteredResult.Error("Outcome/Technical_Error");
                }

                var viewModel = JsonConvert.DeserializeObject<ApplicationConfirmationEntitledViewModel>(applicationJson);
                if (viewModel == null)
                {
                    _logger.LogWarning("Deserialization returned null for application");
                    return AdminAppealsRegisteredResult.Error("Outcome/Technical_Error");
                }

                // Validate required properties
                if (string.IsNullOrEmpty(viewModel.ParentName))
                {
                    _logger.LogWarning("viewModel.ParentName is null or empty");
                }

                if (viewModel.Children == null)
                {
                    _logger.LogWarning("viewModel.Children is null. Initializing to an empty list");
                    viewModel.Children = new List<ApplicationConfirmationEntitledChildViewModel>();
                }

                _logger.LogInformation("Successfully processed admin appeal for parent: {parentName}",
                    viewModel.ParentName);

                return AdminAppealsRegisteredResult.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing admin appeals registration");
                return AdminAppealsRegisteredResult.Error("Outcome/Technical_Error");
            }
        }
    }
}