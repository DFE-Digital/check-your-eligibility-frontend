using CheckYourEligibility_FrontEnd.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public class AdminEnterChildDetailsResult
    {
        public Children Children { get; set; }
        public bool IsRedirect { get; set; }
        public ModelStateDictionary ModelState { get; set; }

        public static AdminEnterChildDetailsResult Success(Children children, bool isRedirect = false, ModelStateDictionary modelState = null) =>
            new() { Children = children, IsRedirect = isRedirect, ModelState = modelState };
    }

    public interface IAdminEnterChildDetailsUseCase
    {
       
        Task<AdminEnterChildDetailsResult> Execute(
            bool? isChildAddOrRemove = null,
            string childListJson = null,
            string fsmApplicationJson = null,
            bool? isRedirect = null);
    }

    public class AdminEnterChildDetailsUseCase : IAdminEnterChildDetailsUseCase
    {
        private readonly ILogger<AdminEnterChildDetailsUseCase> _logger;

        public AdminEnterChildDetailsUseCase(ILogger<AdminEnterChildDetailsUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AdminEnterChildDetailsResult> Execute(
            bool? isChildAddOrRemove = null,
            string childListJson = null,
            string fsmApplicationJson = null,
            bool? isRedirect = null)
        {
            try
            {
                await Task.CompletedTask; // Maintains async contract for future extensions

                // Initialize default model with one empty child
                var children = new Children { ChildList = new List<Child> { new Child() } };

                // Handle redirect after add/remove child operations
                if (isChildAddOrRemove == true && !string.IsNullOrEmpty(childListJson))
                {
                    _logger.LogInformation("Processing child add/remove redirect with child list JSON");

                    try
                    {
                        var childList = JsonConvert.DeserializeObject<List<Child>>(childListJson);
                        if (childList != null && childList.Any())
                        {
                            children.ChildList = childList;
                            _logger.LogInformation("Successfully deserialized child list with {count} children", childList.Count);
                        }
                        else
                        {
                            _logger.LogWarning("Deserialized child list was null or empty, using default model");
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Error deserializing child list JSON");
                        // On error, return default model with single empty child
                        children.ChildList = new List<Child> { new Child() };
                    }

                    // Create new ModelState for add/remove redirects
                    var modelState = new ModelStateDictionary();
                    return AdminEnterChildDetailsResult.Success(children, false, modelState);
                }

                // Handle redirect from FSM application
                if (isRedirect == true && !string.IsNullOrEmpty(fsmApplicationJson))
                {
                    _logger.LogInformation("Processing redirect from FSM application");
                    try
                    {
                        var fsmApplication = JsonConvert.DeserializeObject<FsmApplication>(fsmApplicationJson);
                        if (fsmApplication?.Children != null && fsmApplication.Children.ChildList.Any())
                        {
                            children = fsmApplication.Children;
                            _logger.LogInformation("Successfully loaded children from FSM application");
                        }
                        else
                        {
                            _logger.LogWarning("FSM application or children were null, using default model");
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Error deserializing FSM application JSON");
                        // On error, return default model with single empty child
                        children.ChildList = new List<Child> { new Child() };
                    }

                    return AdminEnterChildDetailsResult.Success(children, true);
                }

                // Default case - initial page load
                _logger.LogInformation("Processing initial page load with default model");
                return AdminEnterChildDetailsResult.Success(children);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Enter_Child_Details");
                throw;
            }
        }
    }
}