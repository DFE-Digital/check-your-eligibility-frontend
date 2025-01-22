using CheckYourEligibility_FrontEnd.Models;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface IAddChildUseCase
    {
        Task<(bool IsSuccess, Children UpdatedChildren)> ExecuteAsync(Children request);
    }

    public class AddChildUseCase : IAddChildUseCase
    {
        private const int MaxChildren = 99;
        private readonly ILogger<AddChildUseCase> _logger;

        public AddChildUseCase(ILogger<AddChildUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<(bool IsSuccess, Children UpdatedChildren)> ExecuteAsync(Children request)
        {
            try
            {
                if (request.ChildList.Count >= MaxChildren)
                {
                    _logger.LogInformation("Maximum number of children ({MaxChildren}) reached", MaxChildren);
                    return Task.FromResult((false, request));
                }

                request.ChildList.Add(new Child());

                _logger.LogInformation("Successfully added new child. Total children: {Count}", request.ChildList.Count);
                return Task.FromResult((true, request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding child");
                throw;
            }
        }
    }
}