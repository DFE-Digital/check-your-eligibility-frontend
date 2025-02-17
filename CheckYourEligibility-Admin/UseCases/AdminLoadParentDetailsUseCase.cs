﻿using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminLoadParentDetailsUseCase
    {
        Task<(ParentGuardian Parent, Dictionary<string, List<string>> ValidationErrors)> Execute(
            string parentDetailsJson = null,
            string validationErrorsJson = null
        );
    }

    public class AdminLoadParentDetailsUseCase : IAdminLoadParentDetailsUseCase
    {
        private readonly ILogger<AdminLoadParentDetailsUseCase> _logger;

        public AdminLoadParentDetailsUseCase(ILogger<AdminLoadParentDetailsUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(ParentGuardian Parent, Dictionary<string, List<string>> ValidationErrors)> Execute(
            string parentDetailsJson = null,
            string validationErrorsJson = null)
        {
            try
            {
                
                ParentGuardian parent = null;
                Dictionary<string, List<string>> errors = null;

                
                if (!string.IsNullOrEmpty(parentDetailsJson))
                {
                    parent = JsonConvert.DeserializeObject<ParentGuardian>(parentDetailsJson);
                    if (parent == null)
                    {
                        _logger.LogWarning("Failed to deserialize parent details");
                    }
                }

                
                if (!string.IsNullOrEmpty(validationErrorsJson))
                {
                    errors = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(validationErrorsJson);
                    if (errors != null)
                    {
                        
                        const string targetValue = "Please select one option";

                        if (errors.ContainsKey("NationalInsuranceNumber") &&
                            errors.ContainsKey("NationalAsylumSeekerServiceNumber"))
                        {
                            if (errors["NationalInsuranceNumber"].Contains(targetValue) &&
                                errors["NationalAsylumSeekerServiceNumber"].Contains(targetValue))
                            {
                                errors.Remove("NationalInsuranceNumber");
                                errors.Remove("NationalAsylumSeekerServiceNumber");
                                errors["NINAS"] = new List<string> { targetValue };
                            }
                        }
                    }
                }

                return (parent, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading parent details");
                throw;
            }
        }
    }
}