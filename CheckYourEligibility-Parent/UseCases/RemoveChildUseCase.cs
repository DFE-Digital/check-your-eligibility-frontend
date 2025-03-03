using CheckYourEligibility_FrontEnd.Models;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface IRemoveChildUseCase
    {
        Children Execute(Children request, int index);
    }
    
    [Serializable]
    public class RemoveChildValidationException : Exception
    {
        
        public RemoveChildValidationException(string message) : base (message)
        {
        }
    }

    public class RemoveChildUseCase : IRemoveChildUseCase
    {
        private readonly ILogger<RemoveChildUseCase> _logger;

        public RemoveChildUseCase(ILogger<RemoveChildUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Children Execute(Children request, int index)
        {
            if (request?.ChildList == null)
            {
                throw new RemoveChildValidationException("Invalid request - no children list available");
            }

            if (index < 0 || index >= request.ChildList.Count)
            {
                throw new RemoveChildValidationException("Invalid child index");
            }

            var child = request.ChildList[index];
            request.ChildList.Remove(child);


            return request;
        }
    }
}