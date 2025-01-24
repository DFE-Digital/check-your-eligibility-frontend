using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

// IMPORTANT: Remove or comment out any blanket "using CheckYourEligibility.Domain.Responses;"
// using CheckYourEligibility.Domain.Responses; // <-- Remove this line if present.

// Instead, explicitly alias only the classes you actually need from that namespace:
using EstablishmentSearchResponse = CheckYourEligibility.Domain.Responses.EstablishmentSearchResponse;
using Establishment = CheckYourEligibility.Domain.Responses.Establishment;
using CheckYourEligibility_FrontEnd.Services;
using System.Text;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class ProcessChildDetailsUseCaseTests
    {
        private ProcessChildDetailsUseCase _sut;
        private Mock<ILogger<ProcessChildDetailsUseCase>> _loggerMock;
        private Mock<IEcsServiceParent> _parentServiceMock;
        private Mock<ISession> _sessionMock;

        [SetUp]
        public void SetUp()
        {
            // Set up Mocks
            _loggerMock = new Mock<ILogger<ProcessChildDetailsUseCase>>();
            _parentServiceMock = new Mock<IEcsServiceParent>();
            _sessionMock = new Mock<ISession>();

            // Configure session mock (if you need to store or retrieve data):
            SetupSessionMock();

            // Instantiate the SUT
            _sut = new ProcessChildDetailsUseCase(
                _loggerMock.Object,
                _parentServiceMock.Object
            );
        }

        private void SetupSessionMock()
        {
            
            var sessionStorage = new Dictionary<string, byte[]>();

            
            sessionStorage["ParentFirstName"] = Encoding.UTF8.GetBytes("John");
            sessionStorage["ParentLastName"] = Encoding.UTF8.GetBytes("Doe");
            sessionStorage["ParentDOB"] = Encoding.UTF8.GetBytes("1990-01-01");
            sessionStorage["ParentNINO"] = Encoding.UTF8.GetBytes("AB123456C");
            sessionStorage["Email"] = Encoding.UTF8.GetBytes("john.doe@test.com");

            
            _sessionMock
                .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) =>
                {
                    if (sessionStorage.TryGetValue(key, out var actualValue))
                    {
                        value = actualValue;
                        return true;
                    }
                    value = null;
                    return false;
                });

           
            _sessionMock
                .Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback((string key, byte[] val) => sessionStorage[key] = val);

            
            _sessionMock
                .Setup(s => s.Remove(It.IsAny<string>()))
                .Callback((string key) => sessionStorage.Remove(key));

            
        }

        [Test]
        public async Task ExecuteAsync_WithValidSchoolUrn_ShouldUpdateSchoolNameAndReturnFsmApplication()
        {
            // Arrange
            var request = new Children
            {
                ChildList = new List<Child>
                {
                    new Child
                    {
                        School = new School { URN = "123456" }
                    }
                }
            };

            // We only need the domain "EstablishmentSearchResponse" and "Establishment":
            var schoolResponse = new EstablishmentSearchResponse
            {
                Data = new List<Establishment>
                {
                    new Establishment { Name = "Test School" }
                }
            };

            _parentServiceMock.Setup(x => x.GetSchool("123456"))
                .ReturnsAsync(schoolResponse);

            // Act
            var result = await _sut.ExecuteAsync(request, _sessionMock.Object, new Dictionary<string, string[]>());

            // Because result.Model is "FsmApplication", cast it:
            var model = (FsmApplication)result;

            model.Children.ChildList.First().School.Name.Should().Be("Test School");
            model.ParentFirstName.Should().Be("John");
            model.ParentLastName.Should().Be("Doe");
        }

        [Test]
        public async Task ExecuteAsync_WithInvalidSchoolUrn_ShouldReturnValidationError()
        {
            // Arrange
            var request = new Children
            {
                ChildList = new List<Child>
                {
                    new Child
                    {
                        School = new School { URN = "12345" }
                    }
                }
            };

            // Act
            await FluentActions.Invoking(() =>_sut.ExecuteAsync(request, _sessionMock.Object, new Dictionary<string, string[]>()))
                .Should().ThrowAsync<ProcessChildDetailsUseCase.ProcessChildDetailsValidationException>("School URN should be a 6 digit number.");
        }

        [Test]
        public async Task ExecuteAsync_WithNonexistentSchool_ShouldReturnValidationError()
        {
            // Arrange
            var request = new Children
            {
                ChildList = new List<Child>
                {
                    new Child
                    {
                        School = new School { URN = "123456" }
                    }
                }
            };

            _parentServiceMock.Setup(x => x.GetSchool("123456"))
                .ReturnsAsync((EstablishmentSearchResponse)null);

            // Act
            await FluentActions.Invoking(() =>_sut.ExecuteAsync(request, _sessionMock.Object, new Dictionary<string, string[]>()))
                .Should().ThrowAsync<ProcessChildDetailsUseCase.ProcessChildDetailsValidationException>("The selected school does not exist in our service.");
        }
    }
}
