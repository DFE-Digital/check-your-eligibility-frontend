using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface IApplicationSentUseCase
    {
        void Execute(ModelStateDictionary modelState);
    }

    public class ApplicationSentUseCase : IApplicationSentUseCase
    {
        public void Execute(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            modelState.Clear();
        }
    }
}