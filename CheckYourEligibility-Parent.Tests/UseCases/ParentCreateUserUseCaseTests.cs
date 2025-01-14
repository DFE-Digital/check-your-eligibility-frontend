using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Moq;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    public class ParentCreateUserUseCaseTests
    {
        private Mock<IEcsServiceParent> _parentServiceMock;
        private ParentCreateUserUseCase _sut;

        [SetUp]
        public void Setup()
        {
            _parentServiceMock = new Mock<IEcsServiceParent>();
            _sut = new ParentCreateUserUseCase(_parentServiceMock.Object);
        }

        [Test]
        public async Task ExecuteAsync_WithValidCredentials_ReturnsUserId()
        {
            // Arrange
            var email = "homer.simpson@springfield.com";
            var uniqueId = "12345";
            var expectedUserId = "user123";

            _parentServiceMock
                .Setup(x => x.CreateUser(It.Is<UserCreateRequest>(r =>
                    r.Data.Email == email &&
                    r.Data.Reference == uniqueId)))
                .ReturnsAsync(new UserSaveItemResponse { Data = expectedUserId });

            // Act
            var result = await _sut.ExecuteAsync(email, uniqueId);

            // Assert
            result.Should().Be(expectedUserId);
        }

        [Test]
        public async Task ExecuteAsync_WithEmptyEmail_ThrowsArgumentException()
        {
            // Act & Assert
            await FluentActions
                .Invoking(() => _sut.ExecuteAsync("", "uniqueId"))
                .Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Email cannot be empty.*");
        }

        [Test]
        public async Task ExecuteAsync_WithNullEmail_ThrowsArgumentException()
        {
            // Act & Assert
            await FluentActions
                .Invoking(() => _sut.ExecuteAsync(null, "uniqueId"))
                .Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Email cannot be empty.*");
        }

        [Test]
        public async Task ExecuteAsync_WithEmptyUniqueId_ThrowsArgumentException()
        {
            // Act & Assert
            await FluentActions
                .Invoking(() => _sut.ExecuteAsync("test@test.com", ""))
                .Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Unique ID cannot be empty.*");
        }

        [Test]
        public async Task ExecuteAsync_WithNullUniqueId_ThrowsArgumentException()
        {
            // Act & Assert
            await FluentActions
                .Invoking(() => _sut.ExecuteAsync("test@test.com", null))
                .Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Unique ID cannot be empty.*");
        }

        [Test]
        public async Task ExecuteAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _parentServiceMock
                .Setup(x => x.CreateUser(It.IsAny<UserCreateRequest>()))
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await FluentActions
                .Invoking(() => _sut.ExecuteAsync("test@test.com", "uniqueId"))
                .Should()
                .ThrowAsync<Exception>()
                .WithMessage("Service error");
        }
    }
}