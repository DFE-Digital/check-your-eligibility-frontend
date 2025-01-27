using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class CreateUserUseCaseTests
    {
        private Mock<IEcsServiceParent> _parentServiceMock;
        private CreateUserUseCase _sut;

        [SetUp]
        public void Setup()
        {
            _parentServiceMock = new Mock<IEcsServiceParent>();
            _sut = new CreateUserUseCase(_parentServiceMock.Object);
        }

        [Test]
        public void Constructor_WhenParentServiceIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            FluentActions.Invoking(() => new CreateUserUseCase(null))
                .Should().Throw<ArgumentNullException>()
                .WithParameterName("parentService");
        }

        [Test]
        public async Task Execute_WhenEmailIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            string email = "";
            string uniqueId = "123abc";

            // Act
            Func<Task> act = () => _sut.Execute(email, uniqueId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Email cannot be empty*")
                .WithParameterName("email");
        }

        [Test]
        public async Task Execute_WhenUniqueIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            string email = "test@example.com";
            string uniqueId = "";

            // Act
            Func<Task> act = () => _sut.Execute(email, uniqueId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Unique ID cannot be empty*")
                .WithParameterName("uniqueId");
        }

        [Test]
        public async Task Execute_WhenEmailIsNull_ShouldThrowArgumentException()
        {
            // Arrange
            string email = null;
            string uniqueId = "123abc";

            // Act
            Func<Task> act = () => _sut.Execute(email, uniqueId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Email cannot be empty*")
                .WithParameterName("email");
        }

        [Test]
        public async Task Execute_WhenUniqueIdIsNull_ShouldThrowArgumentException()
        {
            // Arrange
            string email = "test@example.com";
            string uniqueId = null;

            // Act
            Func<Task> act = () => _sut.Execute(email, uniqueId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Unique ID cannot be empty*")
                .WithParameterName("uniqueId");
        }

        [Test]
        public async Task Execute_WhenServiceReturnsNull_ShouldThrowException()
        {
            // Arrange
            var email = "test@example.com";
            var uniqueId = "123abc";

            _parentServiceMock.Setup(x => x.CreateUser(It.IsAny<UserCreateRequest>()))
                .ReturnsAsync((UserSaveItemResponse)null);

            // Act
            Func<Task> act = () => _sut.Execute(email, uniqueId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Failed to create user");
        }

        [Test]
        public async Task Execute_WhenServiceReturnsNullData_ShouldThrowException()
        {
            // Arrange
            var email = "test@example.com";
            var uniqueId = "123abc";
            var response = new UserSaveItemResponse { Data = null };

            _parentServiceMock.Setup(x => x.CreateUser(It.IsAny<UserCreateRequest>()))
                .ReturnsAsync(response);

            // Act
            Func<Task> act = () => _sut.Execute(email, uniqueId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Failed to create user");
        }

        [Test]
        public async Task Execute_WhenSuccessful_ShouldReturnUserId()
        {
            // Arrange
            var email = "test@example.com";
            var uniqueId = "123abc";
            var expectedUserId = "user123";
            var response = new UserSaveItemResponse { Data = expectedUserId };

            _parentServiceMock.Setup(x => x.CreateUser(It.Is<UserCreateRequest>(r =>
                    r.Data.Email == email &&
                    r.Data.Reference == uniqueId)))
                .ReturnsAsync(response);

            // Act
            var result = await _sut.Execute(email, uniqueId);

            // Assert
            result.Should().Be(expectedUserId);
            _parentServiceMock.Verify(x => x.CreateUser(It.Is<UserCreateRequest>(r =>
                r.Data.Email == email &&
                r.Data.Reference == uniqueId)), Times.Once);
        }
    }
}