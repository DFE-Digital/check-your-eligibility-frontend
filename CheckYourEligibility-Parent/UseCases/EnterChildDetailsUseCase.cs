using CheckYourEligibility_FrontEnd.Models;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface IEnterChildDetailsUseCase
    {
        Task<Children> ExecuteAsync(string childListJson, bool? isChildAddOrRemove);
    }

    public class EnterChildDetailsUseCase : IEnterChildDetailsUseCase
    {
        private readonly ILogger<EnterChildDetailsUseCase> _logger;

        public EnterChildDetailsUseCase(ILogger<EnterChildDetailsUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Children> ExecuteAsync(string childListJson, bool? isChildAddOrRemove)
        {
            try
            {
                var children = new Children { ChildList = new List<Child> { new Child() } };

                if (isChildAddOrRemove == true && !string.IsNullOrEmpty(childListJson))
                {
                    var deserializedChildren = JsonConvert.DeserializeObject<List<Child>>(childListJson);
                    if (deserializedChildren != null)
                    {
                        children.ChildList = deserializedChildren;
                    }
                }

                return Task.FromResult(children);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing child details");
                throw;
            }
        }
    }
}