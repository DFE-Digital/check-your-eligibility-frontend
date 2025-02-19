using CheckYourEligibility_FrontEnd.Models;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminRemoveChildUseCase
    {
        Task<Children> Execute(Children request, int index);
    }

    public class AdminRemoveChildUseCase : IAdminRemoveChildUseCase
    {
        public Task<Children> Execute(Children request, int index)
        {
            try
            {
                var child = request.ChildList[index];
                request.ChildList.Remove(child);
                return Task.FromResult(request);
            }
            catch (IndexOutOfRangeException)
            {
                throw;
            }
        }
    }
}