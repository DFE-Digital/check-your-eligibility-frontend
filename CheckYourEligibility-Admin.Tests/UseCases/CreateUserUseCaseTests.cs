using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CheckYourEligibility_.Tests.UseCases
{
    [TestFixture]
    public class CreateUserUseCaseTests
    {
        private CreateUserUseCase _sut;
        private Mock<ILogger<CreateUserUseCase>> _loggerMock;
        private Mock<IEcsServiceParent> _parentServiceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<CreateUserUseCase>>();
            _parentServiceMock = new Mock<IEcsServiceParent>();
            _sut = new CreateUserUseCase(_loggerMock.Object, _parentServiceMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Execute_WithValidClaims_CreatesUser()
        {
            // Arrange
            var claims = CreateValidDfeClaims();
            var response = new UserSaveItemResponse { Data = "user123" };

            _parentServiceMock
                .Setup(x => x.CreateUser(It.IsAny<UserCreateRequest>()))
                .ReturnsAsync(response);

            // Act
            var result = await _sut.Execute(claims);

            // Assert
            result.Should().Be(response.Data);
            _parentServiceMock.Verify(x => x.CreateUser(It.IsAny<UserCreateRequest>()), Times.Once);
        }

        private IEnumerable<Claim> CreateValidDfeClaims()
        {
            // Ensure these claims include all required types that DfeSignInExtensions.GetDfeClaims expects.
            return new List<Claim>
            {
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "user123"),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@example.com"),
                // Additional claims that might be required by your DfE Sign-In logic:
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User")
            };
        }
    }
}
