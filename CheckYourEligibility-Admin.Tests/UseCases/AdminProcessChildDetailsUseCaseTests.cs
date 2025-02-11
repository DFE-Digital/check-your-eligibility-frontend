using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using ModelChild = CheckYourEligibility_FrontEnd.Models.Child;
using static CheckYourEligibility_FrontEnd.UseCases.Admin.AdminProcessChildDetailsUseCase;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminProcessChildDetailsUseCaseTests
    {
        private Mock<ILogger<AdminProcessChildDetailsUseCase>> _loggerMock;
        private Mock<IEcsServiceParent> _parentServiceMock;
        private Mock<ISession> _sessionMock;
        private AdminProcessChildDetailsUseCase _sut;
        private Dictionary<string, byte[]> _sessionData;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminProcessChildDetailsUseCase>>();
            _parentServiceMock = new Mock<IEcsServiceParent>();
            _sessionMock = new Mock<ISession>();
            _sessionData = new Dictionary<string, byte[]>();

            _sut = new AdminProcessChildDetailsUseCase(
                _loggerMock.Object,
                _parentServiceMock.Object);

            SetupSessionMock();
        }

        private void SetupSessionMock()
        {
            _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) =>
                {
                    if (_sessionData.TryGetValue(key, out var data))
                    {
                        value = data;
                        return true;
                    }
                    value = null;
                    return false;
                });

            void SetSessionValue(string key, string value) =>
                _sessionData[key] = Encoding.UTF8.GetBytes(value);

            SetSessionValue("ParentFirstName", "TestFirst");
            SetSessionValue("ParentLastName", "TestLast");
            SetSessionValue("ParentDOB", "1990-01-01");
            SetSessionValue("ParentNINO", "AB123456C");
            SetSessionValue("ParentEmail", "test@example.com");
        }

        [Test]
        public async Task Execute_WithValidRequest_ShouldCreateFsmApplication()
        {
            // Arrange
            var request = new Children
            {
                ChildList = new List<ModelChild>
                {
                    new ModelChild { FirstName = "TestChild", LastName = "TestLast" }
                }
            };
            var validationErrors = new Dictionary<string, string[]>();

            // Act
            var result = await _sut.Execute(request, _sessionMock.Object, validationErrors);

            // Assert
            result.Should().NotBeNull();
            result.ParentFirstName.Should().Be("TestFirst");
            result.ParentLastName.Should().Be("TestLast");
            result.Children.Should().BeEquivalentTo(request);
        }

        [Test]
        public async Task Execute_WithValidationErrors_ShouldThrowException()
        {
            // Arrange
            var request = new Children
            {
                ChildList = new List<ModelChild>
                {
                    new ModelChild { FirstName = "TestChild", LastName = "TestLast" }
                }
            };
            var validationErrors = new Dictionary<string, string[]>
            {
                { "TestField", new[] { "Test error" } }
            };

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(request, _sessionMock.Object, validationErrors))
                .Should().ThrowAsync<AdminProcessChildDetailsValidationException>();
        }

        [Test]
        public async Task Execute_WithNullRequest_ShouldThrowValidationException()
        {
            // Arrange
            var validationErrors = new Dictionary<string, string[]>();

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(null, _sessionMock.Object, validationErrors))
                .Should().ThrowAsync<AdminProcessChildDetailsValidationException>()
                .WithMessage("{\"Children\":[\"Child details are required\"]}");
        }

        [Test]
        public async Task Execute_WhenServiceThrows_ShouldThrowValidationException()
        {
            // Arrange
            var request = new Children
            {
                ChildList = new List<ModelChild>
        {
            new ModelChild
            {
                ChildIndex = 1,
                FirstName = "TestChild",
                LastName = "TestLast",
                Day = "1",
                Month = "1",
                Year = "2020",
                School = new School
                {
                    URN = "123456",
                    Name = "Test School"
                }
            }
        }
            };

            // Setup GetSchool to throw when validating the school
            _parentServiceMock
                .Setup(x => x.GetSchool("123456"))
                .ThrowsAsync(new Exception("Service error"));

            var validationErrors = new Dictionary<string, string[]>();

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(request, _sessionMock.Object, validationErrors))
                .Should().ThrowAsync<AdminProcessChildDetailsValidationException>()
                .WithMessage("{\"School_1\":[\"An error occurred validating the school\"]}");

            // Verify the service method was called
            _parentServiceMock.Verify(x => x.GetSchool("123456"), Times.Once);

            // Verify the error was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error validating school for child 1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task Execute_WithInvalidSchool_ShouldThrowValidationException()
        {
            // Arrange
            var request = new Children
            {
                ChildList = new List<ModelChild>
        {
            new ModelChild
            {
                ChildIndex = 1,
                FirstName = "TestChild",
                LastName = "TestLast",
                Day = "1",
                Month = "1",
                Year = "2020",
                School = new School
                {
                    URN = "123456",
                    Name = "Test School"
                }
            }
        }
            };

            // Setup GetSchool to return empty list indicating school not found
            _parentServiceMock
                .Setup(x => x.GetSchool("123456"))
                .ReturnsAsync(new EstablishmentSearchResponse { Data = new List<Establishment>() });

            var validationErrors = new Dictionary<string, string[]>();

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Execute(request, _sessionMock.Object, validationErrors))
                .Should().ThrowAsync<AdminProcessChildDetailsValidationException>()
                .WithMessage("{\"School_1\":[\"The selected school does not exist in our service\"]}");

            // Verify the service method was called
            _parentServiceMock.Verify(x => x.GetSchool("123456"), Times.Once);
        }

        [Test]
        public async Task Execute_ShouldLogInformation()
        {
            // Arrange
            var request = new Children
            {
                ChildList = new List<ModelChild>
                {
                    new ModelChild { FirstName = "TestChild", LastName = "TestLast" }
                }
            };
            var validationErrors = new Dictionary<string, string[]>();

            // Act
            await _sut.Execute(request, _sessionMock.Object, validationErrors);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Creating new FSM application")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}