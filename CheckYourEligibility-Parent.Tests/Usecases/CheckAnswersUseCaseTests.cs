using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DomainChild = CheckYourEligibility.Domain.Responses.Child;
using ModelChild = CheckYourEligibility_FrontEnd.Models.Child;
using ModelSchool = CheckYourEligibility_FrontEnd.Models.School;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class CheckAnswersUseCaseTests
    {
        private Mock<ILogger<CheckAnswersUseCase>> _loggerMock;
        private Mock<IEcsServiceParent> _parentServiceMock;
        private CheckAnswersUseCase _sut;
        private FsmApplication _fsmApplication;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<CheckAnswersUseCase>>();
            _parentServiceMock = new Mock<IEcsServiceParent>();
            _sut = new CheckAnswersUseCase(_loggerMock.Object, _parentServiceMock.Object);

            _fsmApplication = new FsmApplication
            {
                ParentFirstName = "Test",
                ParentLastName = "Parent",
                ParentDateOfBirth = "1990-01-01",
                Children = new Children
                {
                    ChildList = new List<ModelChild>
                    {
                        new ModelChild
                        {
                            FirstName = "Test",
                            LastName = "Child",
                            Day = "1",
                            Month = "1",
                            Year = "2015",
                            School = new ModelSchool { URN = "123456" }
                        }
                    }
                }
            };
        }

        [Test]
        public async Task ProcessApplicationAsync_WhenStatusIsEligible_ShouldProcessApplication()
        {
            // Arrange
            _parentServiceMock
                .Setup(x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()))
                .ReturnsAsync(new ApplicationSaveItemResponse());

            // Act
            var result = await _sut.ProcessApplicationAsync(
                _fsmApplication,
                CheckEligibilityStatus.eligible.ToString(),
                "userId",
                "test@example.com");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Responses.Should().HaveCount(1);
            result.ErrorMessage.Should().BeNull();

            _parentServiceMock.Verify(
                x => x.PostApplication_Fsm(It.Is<ApplicationRequest>(r =>
                    r.Data.ParentFirstName == "Test" &&
                    r.Data.ChildFirstName == "Test" &&
                    r.Data.ChildDateOfBirth == "2015-01-01")),
                Times.Once);
        }

        [Test]
        public async Task ProcessApplicationAsync_WhenStatusIsNotEligible_ShouldReturnError()
        {
            // Act
            var result = await _sut.ProcessApplicationAsync(
                _fsmApplication,
                CheckEligibilityStatus.notEligible.ToString(),
                "userId",
                "test@example.com");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Responses.Should().BeNull();
            result.ErrorMessage.Should().Contain("Invalid status");

            _parentServiceMock.Verify(
                x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()),
                Times.Never);
        }

        [Test]
        public async Task ProcessApplicationAsync_WithMultipleChildren_ShouldProcessAllApplications()
        {
            // Arrange
            _fsmApplication.Children.ChildList.Add(new ModelChild
            {
                FirstName = "Test2",
                LastName = "Child2",
                Day = "1",
                Month = "1",
                Year = "2017",
                School = new ModelSchool { URN = "123456" }
            });

            _parentServiceMock
                .Setup(x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()))
                .ReturnsAsync(new ApplicationSaveItemResponse());

            // Act
            var result = await _sut.ProcessApplicationAsync(
                _fsmApplication,
                CheckEligibilityStatus.eligible.ToString(),
                "userId",
                "test@example.com");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Responses.Should().HaveCount(2);

            _parentServiceMock.Verify(
                x => x.PostApplication_Fsm(It.Is<ApplicationRequest>(r =>
                    r.Data.ParentFirstName == "Test")),
                Times.Exactly(2));
        }

        [Test]
        public async Task ProcessApplicationAsync_WhenServiceThrows_ShouldPropagateException()
        {
            // Arrange
            _parentServiceMock
                .Setup(x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.ProcessApplicationAsync(
                    _fsmApplication,
                    CheckEligibilityStatus.eligible.ToString(),
                    "userId",
                    "test@example.com"))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Test exception");
        }

        [Test]
        public async Task ProcessApplicationAsync_WithValidApplication_ShouldMapFieldsCorrectly()
        {
            // Arrange
            ApplicationRequest capturedRequest = null;
            _parentServiceMock
                .Setup(x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()))
                .Callback<ApplicationRequest>(request => capturedRequest = request)
                .ReturnsAsync(new ApplicationSaveItemResponse());

            // Act
            await _sut.ProcessApplicationAsync(
                _fsmApplication,
                CheckEligibilityStatus.eligible.ToString(),
                "userId",
                "test@example.com");

            // Assert
            capturedRequest.Should().NotBeNull();
            capturedRequest.Data.Should().NotBeNull();
            capturedRequest.Data.ParentFirstName.Should().Be("Test");
            capturedRequest.Data.ParentLastName.Should().Be("Parent");
            capturedRequest.Data.ParentDateOfBirth.Should().Be("1990-01-01");
            capturedRequest.Data.ChildFirstName.Should().Be("Test");
            capturedRequest.Data.ChildLastName.Should().Be("Child");
            capturedRequest.Data.ChildDateOfBirth.Should().Be("2015-01-01");
            capturedRequest.Data.Establishment.Should().Be(123456);
            capturedRequest.Data.UserId.Should().Be("userId");
            capturedRequest.Data.ParentEmail.Should().Be("test@example.com");
        }
    }
}