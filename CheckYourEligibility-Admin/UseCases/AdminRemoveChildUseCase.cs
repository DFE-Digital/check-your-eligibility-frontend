using CheckYourEligibility_FrontEnd.Models;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminRemoveChildUseCase
    {
        Task<Children> Execute(Children request, int index);
    }

    [Serializable]
    public class AdminRemoveChildException : Exception
    {
        public AdminRemoveChildException(string message) : base(message)
        {
        }
    }

    public class AdminRemoveChildUseCase : IAdminRemoveChildUseCase
    {
        private readonly ILogger<AdminRemoveChildUseCase> _logger;

        public AdminRemoveChildUseCase(ILogger<AdminRemoveChildUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Children> Execute(Children request, int index)
        {
            try
            {
                ValidateRequest(request, index);

                var child = request.ChildList[index];
                request.ChildList.Remove(child);

                _logger.LogInformation("Successfully removed child at index {Index}. Remaining children: {Count}",
                    index, request.ChildList.Count);

                return request;
            }
            catch (Exception ex) when (ex is not AdminRemoveChildException)
            {
                _logger.LogError(ex, "Failed to remove child at index {Index}", index);
                throw new AdminRemoveChildException($"Failed to remove child: {ex.Message}");
            }
        }

        private void ValidateRequest(Children request, int index)
        {
            if (request?.ChildList == null)
            {
                throw new AdminRemoveChildException("Invalid request - no children list available");
            }

            if (index < 0 || index >= request.ChildList.Count)
            {
                throw new AdminRemoveChildException($"Invalid child index: {index}");
            }
        }
    }
}