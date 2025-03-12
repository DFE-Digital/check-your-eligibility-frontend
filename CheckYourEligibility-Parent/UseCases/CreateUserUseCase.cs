using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility_FrontEnd.Services;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface ICreateUserUseCase
    {
        Task<string> Execute(string email, string uniqueId);
    }

    public class CreateUserUseCase : ICreateUserUseCase
    {
        private readonly IEcsServiceParent _parentService;

        public CreateUserUseCase(IEcsServiceParent parentService)
        {
            _parentService = parentService ?? throw new ArgumentNullException(nameof(parentService));
        }

        public async Task<string> Execute(string email, string uniqueId)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            if (string.IsNullOrEmpty(uniqueId))
                throw new ArgumentException("Unique ID cannot be empty", nameof(uniqueId));

            var userRequest = new UserCreateRequest
            {
                Data = new UserData
                {
                    Email = email,
                    Reference = uniqueId
                }
            };

            var response = await _parentService.CreateUser(userRequest);
            return response?.Data ?? throw new Exception("Failed to create user");
        }
    }
}