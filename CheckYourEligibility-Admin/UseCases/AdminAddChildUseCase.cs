using CheckYourEligibility_FrontEnd.Models;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminAddChildUseCase
    {
        Children Execute(Children request);
    }

    public class AdminAddChildUseCase : IAdminAddChildUseCase
    {
        private readonly ILogger<AdminAddChildUseCase> _logger;

        public AdminAddChildUseCase(ILogger<AdminAddChildUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Children Execute(Children request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.ChildList == null)
            {
                request.ChildList = new List<Child>();
            }

            if (request.ChildList.Count >= 99)
            {
                return request;
            }

            request.ChildList.Add(new Child());
            return request;
        }
    }
}