using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    public interface IAdminCreateUserUseCase
    {
        Task<string> Execute(IEnumerable<Claim> claims);
    }

    public class AdminCreateUserResult
    {
        public bool IsSuccess { get; set; }
        public string? UserId { get; set; }
        public string? ErrorMessage { get; set; }

        public static AdminCreateUserResult Success(string userId) =>
            new() { IsSuccess = true, UserId = userId };

        public static AdminCreateUserResult Error(string message) =>
            new() { IsSuccess = false, ErrorMessage = message };
    }

    [Serializable]
    public class AdminCreateUserException : Exception
    {
        public AdminCreateUserException(string message) : base(message) { }
    }

    public class AdminCreateUserUseCase : IAdminCreateUserUseCase
    {
        private readonly ILogger<AdminCreateUserUseCase> _logger;
        private readonly IEcsServiceParent _parentService;

        public AdminCreateUserUseCase(
            ILogger<AdminCreateUserUseCase> logger,
            IEcsServiceParent parentService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentService = parentService ?? throw new ArgumentNullException(nameof(parentService));
        }

        public async Task<string> Execute(IEnumerable<Claim> claims)
        {
            try
            {
                var dfeClaims = DfeSignInExtensions.GetDfeClaims(claims);
                if (dfeClaims?.User == null)
                {
                    throw new AdminCreateUserException("DFE user claims not found");
                }

                var userRequest = new UserCreateRequest
                {
                    Data = new UserData
                    {
                        Email = dfeClaims.User.Email,
                        Reference = dfeClaims.User.Id
                    }
                };

                _logger.LogInformation("Creating user with email {Email}", dfeClaims.User.Email);

                var response = await _parentService.CreateUser(userRequest);
                if (response?.Data == null)
                {
                    throw new AdminCreateUserException("User creation response was null");
                }

                return response.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user");
                throw new AdminCreateUserException($"Failed to create user: {ex.Message}");
            }
        }
    }
}