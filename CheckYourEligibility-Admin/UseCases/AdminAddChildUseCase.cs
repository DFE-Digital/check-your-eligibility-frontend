using System.Drawing;
using System.Runtime.Serialization;
using CheckYourEligibility_FrontEnd.Models;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminAddChildUseCase
    {
        Task<Children> Execute(Children request);
    }

    [Serializable]
    public class MaxChildrenException : Exception
    {

        public MaxChildrenException(string message) : base(message)
        {
        }
    }

    public class AdminAddChildUseCase : IAdminAddChildUseCase
    {
        private readonly ILogger<AdminAddChildUseCase> _logger;
        private readonly IConfiguration _configuration;

        public AdminAddChildUseCase(ILogger<AdminAddChildUseCase> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Task<Children> Execute(Children request)
        {
            if (request.ChildList.Count >= _configuration.GetValue<int>("MaxChildren"))
            {
                throw new MaxChildrenException("");
            }

            request.ChildList.Add(new Child()
                );

            _logger.LogInformation("Successfully added new child. Total children: {Count}", request.ChildList.Count);
            return Task.FromResult(request);
        }
    }
}