using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface IProcessChildDetailsUseCase
    {
        Task<(bool IsSuccess, string View, object Model, Dictionary<string, string[]> ValidationErrors)> ExecuteAsync(
            Children request,
            bool isRedirect,
            ISession session,
            Dictionary<string, string[]> existingValidationErrors = null);
    }

    public class ProcessChildDetailsUseCase : IProcessChildDetailsUseCase
    {
        private readonly ILogger<ProcessChildDetailsUseCase> _logger;
        private readonly IEcsServiceParent _parentService;

        public ProcessChildDetailsUseCase(
            ILogger<ProcessChildDetailsUseCase> logger,
            IEcsServiceParent parentService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentService = parentService ?? throw new ArgumentNullException(nameof(parentService));
        }

        public async Task<(bool IsSuccess, string View, object Model, Dictionary<string, string[]> ValidationErrors)> ExecuteAsync(
            Children request,
            bool isRedirect,
            ISession session,
            Dictionary<string, string[]> existingValidationErrors = null)
        {
            try
            {
                var validationErrors = existingValidationErrors ?? new Dictionary<string, string[]>();

                // If it's a redirect with existing data, return early
                if (isRedirect)
                {
                    return (true, "Enter_Child_Details", request, null);
                }

                // Validate schools
                await ValidateSchools(request, validationErrors);

                // If there are validation errors, return early
                if (validationErrors.Any())
                {
                    return (false, "Enter_Child_Details", request, validationErrors);
                }

                // Create FSM application
                var fsmApplication = CreateFsmApplication(request, session);

                return (true, "Check_Answers", fsmApplication, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing child details");
                throw;
            }
        }

        private async Task ValidateSchools(Children request, Dictionary<string, string[]> validationErrors)
        {
            var idx = 0;
            foreach (var item in request.ChildList)
            {
                if (item.School?.URN == null)
                {
                    idx++;
                    continue;
                }

                if (item.School.URN.Length == 6 && int.TryParse(item.School.URN, out _))
                {
                    var schools = await _parentService.GetSchool(item.School.URN);
                    if (schools != null)
                    {
                        item.School.Name = schools.Data.First().Name;
                    }
                    else
                    {
                        AddValidationError(validationErrors, $"ChildList[{idx}].School.URN",
                            "The selected school does not exist in our service.");
                    }
                }
                else
                {
                    AddValidationError(validationErrors, $"ChildList[{idx}].School.URN",
                        "School URN should be a 6 digit number.");
                }
                idx++;
            }
        }

        private void AddValidationError(Dictionary<string, string[]> validationErrors, string key, string message)
        {
            if (!validationErrors.ContainsKey(key))
            {
                validationErrors[key] = new[] { message };
            }
            else
            {
                var errors = validationErrors[key].ToList();
                errors.Add(message);
                validationErrors[key] = errors.ToArray();
            }
        }

        private FsmApplication CreateFsmApplication(Children request, ISession session)
        {
            return new FsmApplication
            {
                ParentFirstName = session.GetString("ParentFirstName"),
                ParentLastName = session.GetString("ParentLastName"),
                ParentDateOfBirth = session.GetString("ParentDOB"),
                ParentNino = session.GetString("ParentNINO"),
                ParentNass = session.GetString("ParentNASS"),
                Children = request,
                Email = session.GetString("Email")
            };
        }
    }
}