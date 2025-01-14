using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility_FrontEnd.Services;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface IParentCreateUserUseCase
    {
        Task<string> ExecuteAsync(string email, string uniqueId);
    }

    public class ParentCreateUserUseCase : IParentCreateUserUseCase
    {
        private readonly IEcsServiceParent _parentService;

        public ParentCreateUserUseCase(IEcsServiceParent parentService)
        {
            _parentService = parentService ?? throw new ArgumentNullException(nameof(parentService));
        }

        public async Task<string> ExecuteAsync(string email, string uniqueId)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be empty.", nameof(email));
            }

            if (string.IsNullOrEmpty(uniqueId))
            {
                throw new ArgumentException("Unique ID cannot be empty.", nameof(uniqueId));
            }

            var response = await _parentService.CreateUser(new UserCreateRequest
            {
                Data = new UserData
                {
                    Email = email,
                    Reference = uniqueId
                }
            });

            return response.Data;
        }
    }
}