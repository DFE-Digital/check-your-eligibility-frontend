using CheckYourEligibility_FrontEnd.Models;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface ILoadParentNassDetailsUseCase
    {
        Task<Parent> ExecuteAsync(string parentDetailsJson = null);
    }

    public class LoadParentNassDetailsUseCase : ILoadParentNassDetailsUseCase
    {
        public async Task<Parent> ExecuteAsync(string parentDetailsJson = null)
        {
            if (string.IsNullOrEmpty(parentDetailsJson))
            {
                return null;
            }

            try
            {
                var parent = JsonConvert.DeserializeObject<Parent>(parentDetailsJson);
                return parent ?? new Parent();
            }
            catch (JsonException)
            {
                // Return a new Parent object if deserialization fails
                return new Parent();
            }
        }
    }
}
