using CheckYourEligibility.FrontEnd.Models;
using CheckYourEligibility.FrontEnd.Gateways.Interfaces;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace CheckYourEligibility.FrontEnd.UseCases
{
    public interface IProcessChildDetailsUseCase
    {
        Task<object> Execute(
            Children request,
            ISession session,
            Dictionary<string, string[]> validationErrors);
    }

    public class ProcessChildDetailsUseCase : IProcessChildDetailsUseCase
    {
        private readonly ILogger<ProcessChildDetailsUseCase> _logger;
        private readonly IParentGateway _parentGatewayService;

        public ProcessChildDetailsUseCase(
            ILogger<ProcessChildDetailsUseCase> logger,
            IParentGateway parentGatewayService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentGatewayService = parentGatewayService ?? throw new ArgumentNullException(nameof(parentGatewayService));
        }
        
        [Serializable]
        public class ProcessChildDetailsValidationException : Exception
        {
        
            public ProcessChildDetailsValidationException(string message) : base(message)
            {
            }
        }

        public async Task<object> Execute(
            Children request,
            ISession session,
            Dictionary<string, string[]> validationErrors)
        {
            // Validate schools
            await ValidateSchools(request, validationErrors);

            // If there are validation errors, return early
            if (validationErrors.Any())
            {
                throw new ProcessChildDetailsValidationException(JsonConvert.SerializeObject(validationErrors));
            }

            // Create FSM application
            var fsmApplication = CreateFsmApplication(request, session);

            return fsmApplication;
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
                    var schools = await _parentGatewayService.GetSchool(item.School.URN);
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