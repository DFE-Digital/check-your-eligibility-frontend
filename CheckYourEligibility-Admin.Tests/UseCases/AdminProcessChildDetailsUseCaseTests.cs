using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility_FrontEnd_Admin.Tests.UseCase
{
    public interface IAdminProcessChildDetailsUseCase
    {
        Task<FsmApplication> Execute(Children request, bool isRedirect, ISession session);
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

        public AdminProcessChildDetailsUseCase(
            ILogger<AdminProcessChildDetailsUseCase> logger,
            IEcsServiceParent parentService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentService = parentService ?? throw new ArgumentNullException(nameof(parentService));
        }

        public async Task<FsmApplication> Execute(Children request, bool isRedirect, ISession session)
        {
            try
            {
                if (request == null)
                {
                    throw new AdminProcessChildDetailsException("Invalid request - children object is null");
                }

                await ValidateSchools(request);

                return CreateFsmApplication(request, session);
            }
            catch (Exception ex) when (ex is not AdminProcessChildDetailsException)
            {
                _logger.LogError(ex, "Failed to process child details");
                throw new AdminProcessChildDetailsException($"Failed to validate school: {ex.Message}");
            }
        }

        private async Task ValidateSchools(Children request)
        {
            foreach (var child in request.ChildList)
            {
                if (child.School?.URN == null) continue;

                if (child.School.URN.Length != 6 || !int.TryParse(child.School.URN, out _))
                {
                    throw new AdminProcessChildDetailsException("School URN should be a 6 digit number");
                }

                var schools = await _parentService.GetSchool(child.School.URN);

                if (schools?.Data == null || !schools.Data.Any())
                {
                    throw new AdminProcessChildDetailsException("The selected school does not exist in our service");
                }

                child.School.Name = schools.Data.First().Name;
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
                ParentEmail = session.GetString("ParentEmail"),
                Children = request
            };
        }
    }
}