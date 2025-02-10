using CheckYourEligibility_FrontEnd.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminEnterChildDetailsUseCase
    {
        Task<Children> Execute(string childListJson, bool? isChildAddOrRemove);
    }

    [Serializable]
    public class AdminEnterChildDetailsException : Exception
    {
        public AdminEnterChildDetailsException(string message) : base(message)
        {
        }
    }

    public class AdminEnterChildDetailsUseCase : IAdminEnterChildDetailsUseCase
    {
        private readonly ILogger<AdminEnterChildDetailsUseCase> _logger;

        public AdminEnterChildDetailsUseCase(ILogger<AdminEnterChildDetailsUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Children> Execute(string childListJson, bool? isChildAddOrRemove)
        {
            try
            {
                _logger.LogInformation("Loading child details with isChildAddOrRemove: {IsChildAddOrRemove}",
                    isChildAddOrRemove);

                if (isChildAddOrRemove == true && !string.IsNullOrEmpty(childListJson))
                {
                    return await LoadExistingChildren(childListJson);
                }

                return CreateDefaultChildren();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enter child details");
                throw new AdminEnterChildDetailsException($"Failed to enter child details: {ex.Message}");
            }
        }

        private async Task<Children> LoadExistingChildren(string childListJson)
        {
            try
            {
                var childList = JsonConvert.DeserializeObject<List<Child>>(childListJson);
                if (childList == null)
                {
                    _logger.LogWarning("Deserialized child list was null");
                    return CreateDefaultChildren();
                }

                return new Children { ChildList = childList };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize child list");
                throw new AdminEnterChildDetailsException("Failed to load existing children");
            }
        }

        private Children CreateDefaultChildren()
        {
            return new Children { ChildList = new List<Child> { new Child() } };
        }
    }
}