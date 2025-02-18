using CheckYourEligibility_FrontEnd.Models;
using Microsoft.AspNetCore.Http;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminProcessChildDetailsUseCase
    {
        Task<FsmApplication> Execute(Children children, ISession session);
    }

    public class AdminProcessChildDetailsUseCase : IAdminProcessChildDetailsUseCase
    {
        public Task<FsmApplication> Execute(Children children, ISession session)
        {
            var fsmApplication = new FsmApplication
            {
                ParentFirstName = session.GetString("ParentFirstName"),
                ParentLastName = session.GetString("ParentLastName"),
                ParentDateOfBirth = session.GetString("ParentDOB"),
                ParentNass = session.GetString("ParentNASS") ?? null,
                ParentNino = session.GetString("ParentNINO") ?? null,
                ParentEmail = session.GetString("ParentEmail"),
                Children = children
            };

            return Task.FromResult(fsmApplication);
        }
    }
}