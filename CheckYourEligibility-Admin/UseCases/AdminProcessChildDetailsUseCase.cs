using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminProcessChildDetailsUseCase
    {
        Task<FsmApplication> Execute(Children request, ISession session, Dictionary<string, string[]> validationErrors);
    }

    [Serializable]
    public class AdminProcessChildDetailsException : Exception
    {
        public AdminProcessChildDetailsException(string message) : base(message)
        {
        }
    }

    public class AdminProcessChildDetailsUseCase : IAdminProcessChildDetailsUseCase
    {
        private readonly ILogger<AdminProcessChildDetailsUseCase> _logger;
        private readonly IEcsServiceParent _parentService;

        public class AdminProcessChildDetailsValidationException : Exception
        {
            public AdminProcessChildDetailsValidationException(string message) : base(message) { }
        }

        public AdminProcessChildDetailsUseCase(
            ILogger<AdminProcessChildDetailsUseCase> logger,
            IEcsServiceParent parentService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentService = parentService ?? throw new ArgumentNullException(nameof(parentService));
        }

        public async Task<FsmApplication> Execute(Children request, ISession session, Dictionary<string, string[]> validationErrors)
        {
            try
            {
                if (validationErrors?.Any() == true)
                {
                    _logger.LogWarning("Validation errors detected in child details");
                    throw new AdminProcessChildDetailsValidationException(
                        JsonConvert.SerializeObject(validationErrors));
                }

                var additionalValidationErrors = await ValidateRequest(request);
                if (additionalValidationErrors.Any())
                {
                    _logger.LogWarning("Additional validation errors found during processing");
                    throw new AdminProcessChildDetailsValidationException(
                        JsonConvert.SerializeObject(additionalValidationErrors));
                }

                _logger.LogInformation("Creating new FSM application with child details");
                return await CreateFsmApplication(request, session);
            }
            catch (Exception ex) when (ex is not AdminProcessChildDetailsValidationException)
            {
                _logger.LogError(ex, "Failed to process child details");
                throw new AdminProcessChildDetailsException($"Failed to process child details: {ex.Message}");
            }
        }

        private async Task<Dictionary<string, string[]>> ValidateRequest(Children request)
        {
            var errors = new Dictionary<string, string[]>();

            // Add business validation rules
            if (request == null)
            {
                errors.Add("Children", new[] { "Child details are required" });
                return errors;
            }

            // Add additional validation logic here
            // Example:
            // if (string.IsNullOrWhiteSpace(request.FirstName))
            // {
            //     errors.Add("FirstName", new[] { "First name is required" });
            // }

            // You can also add async validations using _parentService if needed
            try
            {
                // Example:
                // var existingChild = await _parentService.CheckExistingChild(request.Id);
                // if (existingChild != null)
                // {
                //     errors.Add("Id", new[] { "Child already exists in the system" });
                // }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during child validation");
                errors.Add("General", new[] { "An error occurred during validation" });
            }

            return errors;
        }

        private async Task<FsmApplication> CreateFsmApplication(Children request, ISession session)
        {
            var application = new FsmApplication
            {
                ParentFirstName = session.GetString("ParentFirstName"),
                ParentLastName = session.GetString("ParentLastName"),
                ParentDateOfBirth = session.GetString("ParentDOB"),
                ParentNass = session.GetString("ParentNASS"),
                ParentNino = session.GetString("ParentNINO"),
                ParentEmail = session.GetString("ParentEmail"),
                Children = request
            };

            // Add any additional async processing if needed
            // Example:
            // await _parentService.EnrichApplicationData(application);

            return application;
        }
    }
}