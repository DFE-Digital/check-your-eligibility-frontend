using AutoFixture;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using CheckYourEligibility_FrontEnd.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class AdminSubmitApplicationUseCaseTests
    {
        private AdminSubmitApplicationUseCase _sut;
        private Mock<ILogger<AdminSubmitApplicationUseCase>> _loggerMock;
        private Mock<IEcsServiceParent> _parentServiceMock;
        private IFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminSubmitApplicationUseCase>>();
            _parentServiceMock = new Mock<IEcsServiceParent>();
            _sut = new AdminSubmitApplicationUseCase(_loggerMock.Object, _parentServiceMock.Object);

            _fixture = new Fixture();

            // Configure the fixture to omit auto-properties
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        private FsmApplication CreateTestApplication()
        {
            return new FsmApplication
            {
                ParentFirstName = "Test",
                ParentLastName = "Parent",
                ParentDateOfBirth = "1990-01-01",
                ParentEmail = "test@example.com",
                ParentNino = "AB123456C",
                Children = new Children
                {
                    ChildList = new List<CheckYourEligibility_FrontEnd.Models.Child>
                    {
                        new CheckYourEligibility_FrontEnd.Models.Child
                        {
                            FirstName = "Test",
                            LastName = "Child",
                            Day = "01",
                            Month = "01",
                            Year = "2020"
                        }
                    }
                }
            };
        }

        private ApplicationSaveItemResponse CreateTestResponse()
        {
            return new ApplicationSaveItemResponse
            {
                Data = new ApplicationResponse
                {
                    ChildFirstName = "Test",
                    ChildLastName = "Child",
                    Reference = "TEST-REF-001",
                    Status = "Entitled"
                }
            };
        }

        [Test]
        public async Task Execute_Should_Submit_Application_For_Each_Child()
        {
            // Arrange
            var request = CreateTestApplication();
            var userId = "test-user";
            var establishment = "12345"; // Valid numeric establishment ID
            var response = CreateTestResponse();

            _parentServiceMock
                .Setup(x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()))
                .ReturnsAsync(response);

            // Act
            var (result, lastResponse) = await _sut.Execute(request, userId, establishment);

            // Assert
            result.Should().NotBeNull();
            result.Children.Should().HaveCount(request.Children.ChildList.Count);
            lastResponse.Should().BeEquivalentTo(response);

            _parentServiceMock.Verify(
                x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()),
                Times.Exactly(request.Children.ChildList.Count));
        }

        [Test]
        public void Execute_Should_Handle_Error_When_Service_Throws()
        {
            // Arrange
            var request = CreateTestApplication();
            var userId = "test-user";
            var establishment = "12345"; // Valid numeric establishment ID

            _parentServiceMock
                .Setup(x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()))
                .Callback<ApplicationRequest>(r => throw new Exception("Service error"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<Exception>(async () =>
                await _sut.Execute(request, userId, establishment));

            exception.Message.Should().Be("Service error");

            _parentServiceMock.Verify(
                x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()),
                Times.Once);
        }

        [Test]
        public void Execute_Should_Throw_FormatException_When_Establishment_Is_Not_Numeric()
        {
            // Arrange
            var request = CreateTestApplication();
            var userId = "test-user";
            var establishment = "invalid-id";

            
            var exception = Assert.ThrowsAsync<FormatException>(async () =>
                await _sut.Execute(request, userId, establishment));

            
            exception.Message.Should().Contain("format");

            
            _parentServiceMock.Verify(
                x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()),
                Times.Never);
        }
    }
}