using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.ViewModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminRegistrationUseCase
    {
        Task<ApplicationConfirmationEntitledViewModel> Execute(string applicationJson);
    }

    public class AdminRegistrationUseCase : IAdminRegistrationUseCase
    {
        private readonly ILogger<AdminRegistrationUseCase> _logger;

        public AdminRegistrationUseCase(ILogger<AdminRegistrationUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<ApplicationConfirmationEntitledViewModel> Execute(string applicationJson)
        {
            var fsmApplication = JsonConvert.DeserializeObject<FsmApplication>(applicationJson);

            var vm = new ApplicationConfirmationEntitledViewModel
            {
                ParentName = $"{fsmApplication.ParentFirstName} {fsmApplication.ParentLastName}",
                Children = new List<ApplicationConfirmationEntitledChildViewModel>()
            };

            if (fsmApplication.Children?.ChildList != null)
            {
                foreach (var child in fsmApplication.Children.ChildList)
                {
                    vm.Children.Add(new ApplicationConfirmationEntitledChildViewModel
                    {
                        ParentName = vm.ParentName,
                        ChildName = $"{child.FirstName} {child.LastName}",
                        Reference = $"-{child.ChildIndex}"
                    });
                }
            }

            _logger.LogInformation("Created registration response for parent {ParentName}", vm.ParentName);
            return Task.FromResult(vm);
        }
    }
}
