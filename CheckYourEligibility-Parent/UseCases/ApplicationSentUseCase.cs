using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface IApplicationSentUseCase
    {
        Task<(string ViewName, object Model)> ExecuteAsync();
    }

    public class ApplicationSentUseCase : IApplicationSentUseCase
    {
        private readonly ILogger<ApplicationSentUseCase> _logger;

        public ApplicationSentUseCase(ILogger<ApplicationSentUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<(string ViewName, object Model)> ExecuteAsync()
        {
            _logger.LogInformation("Displaying application sent confirmation page");
            return Task.FromResult(("Application_Sent", (object)null));
        }
    }
}