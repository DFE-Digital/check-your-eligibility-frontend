using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminCreateUserUseCaseTests
    {
        private Mock<ILogger<AdminCreateUserUseCase>> _loggerMock;
        private Mock<IEcsServiceParent> _parentServiceMock;
        private AdminCreateUserUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminCreateUserUseCase>>();
            _parentServiceMock = new Mock<IEcsServiceParent>();
            _sut = new AdminCreateUserUseCase(_loggerMock.Object, _parentServiceMock.Object);
        }

        [Test]
        public async Task Execute_WithValidClaims_ShouldReturnUserId()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("email", "test@example.com"),
                new Claim("id", "12345")
            };

            var expectedResponse = new UserSaveItemResponse
            {
                Data = "user123"
            };

            _parentServiceMock
                .Setup(x => x.CreateUser(It.IsAny<UserCreateRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.Execute(claims);

            // Assert
            result.Should().Be("user123");
            _parentServiceMock.Verify(x => x.CreateUser(It.Is<UserCreateRequest>(r =>
                r.Data.Email == "test@example.com" &&
                r.Data.Reference == "12345")), Times.Once);
        }

        [Test]
        public async Task Execute_WhenServiceReturnsNull_ShouldThrowException()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("email", "test@example.com"),
                new Claim("id", "12345")
            };

            _parentServiceMock
                .Setup(x => x.CreateUser(It.IsAny<UserCreateRequest>()))
                .ReturnsAsync((UserSaveItemResponse)null);

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(claims))
                .Should().ThrowAsync<AdminCreateUserException>()
                .WithMessage("Failed to create user: User creation response was null");
        }

        [Test]
        public async Task Execute_WithMissingEmailClaim_ShouldThrowException()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("id", "12345")
            };

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(claims))
                .Should().ThrowAsync<AdminCreateUserException>()
                .WithMessage("Failed to create user: Required claims not found");
        }

        [Test]
        public async Task Execute_WithMissingIdClaim_ShouldThrowException()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("email", "test@example.com")
            };

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(claims))
                .Should().ThrowAsync<AdminCreateUserException>()
                .WithMessage("Failed to create user: Required claims not found");
        }

        [Test]
        public async Task Execute_WhenServiceThrowsException_ShouldThrowWrappedException()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("email", "test@example.com"),
                new Claim("id", "12345")
            };

            _parentServiceMock
                .Setup(x => x.CreateUser(It.IsAny<UserCreateRequest>()))
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(claims))
                .Should().ThrowAsync<AdminCreateUserException>()
                .WithMessage("Failed to create user: Service error");
        }

        [Test]
        public async Task Execute_ShouldLogInformation()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("email", "test@example.com"),
                new Claim("id", "12345")
            };

            _parentServiceMock
                .Setup(x => x.CreateUser(It.IsAny<UserCreateRequest>()))
                .ReturnsAsync(new UserSaveItemResponse { Data = "user123" });

            // Act
            await _sut.Execute(claims);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Creating user with email")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}