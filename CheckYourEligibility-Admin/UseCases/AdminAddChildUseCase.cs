using System;
using System.Threading.Tasks;
using CheckYourEligibility_FrontEnd.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

    [Serializable]
    public class AddChildException : Exception
    {
        public AddChildException(string message) : base(message) { }
    }

    [Serializable]
    public class AdminAddChildException : Exception
    {
        public AdminAddChildException(string message) : base(message) { }
    }

    public class AdminAddChildUseCase : IAdminAddChildUseCase
    {
        private readonly ILogger<AdminAddChildUseCase> _logger;
        private readonly IConfiguration _configuration;

        public AdminAddChildUseCase(ILogger<AdminAddChildUseCase> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Task<Children> Execute(Children request)
        {
            try
            {
                if (request?.ChildList == null)
                {
                    throw new AdminAddChildException("Failed to add child: Object reference not set to an instance of an object.");
                }

                var maxChildren = _configuration.GetValue<int>("MaxChildren");
                if (request.ChildList.Count >= maxChildren)
                {
                    throw new AddChildException($"Maximum limit of {maxChildren} children reached");
                }

                request.ChildList.Add(new Child());
                _logger.LogInformation("Successfully added new child. Total children: {Count}", request.ChildList.Count);
                return Task.FromResult(request);
            }
            catch (Exception ex) when (ex is not AddChildException && ex is not AdminAddChildException)
            {
                throw new AdminAddChildException($"Failed to add child: {ex.Message}");
            }
        }
    }
}