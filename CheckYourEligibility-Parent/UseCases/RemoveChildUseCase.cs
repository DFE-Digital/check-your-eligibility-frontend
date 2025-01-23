using CheckYourEligibility_FrontEnd.Models;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface IRemoveChildUseCase
    {
        Task<(bool IsSuccess, Children UpdatedChildren, string ErrorMessage)> ExecuteAsync(Children request, int index);
    }

    public class RemoveChildUseCase : IRemoveChildUseCase
    {
        private readonly ILogger<RemoveChildUseCase> _logger;

        public RemoveChildUseCase(ILogger<RemoveChildUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<(bool IsSuccess, Children UpdatedChildren, string ErrorMessage)> ExecuteAsync(Children request, int index)
        {
            try
            {
                if (request?.ChildList == null)
                {
                    _logger.LogWarning("Attempt to remove child from null or empty list");
                    return Task.FromResult((false, request, "Invalid request - no children list available"));
                }

                if (index < 0 || index >= request.ChildList.Count)
                {
                    _logger.LogWarning("Attempt to remove child with invalid index: {Index}", index);
                    return Task.FromResult((false, request, "Invalid child index"));
                }

                var child = request.ChildList[index];
                request.ChildList.Remove(child);

                _logger.LogInformation("Successfully removed child at index {Index}. Remaining children: {Count}",
                    index, request.ChildList.Count);

                return Task.FromResult((true, request, string.Empty));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing child at index {Index}", index);
                throw;
            }
        }
    }
}