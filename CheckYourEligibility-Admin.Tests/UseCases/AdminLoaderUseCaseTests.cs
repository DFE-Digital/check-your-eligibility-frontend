using AutoFixture;
using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminLoaderUseCaseTests
    {
        private AdminLoaderUseCase _sut;
        private Mock<ILogger<AdminLoaderUseCase>> _loggerMock;
        private Mock<IEcsCheckService> _checkServiceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminLoaderUseCase>>();
            _checkServiceMock = new Mock<IEcsCheckService>();
            _sut = new AdminLoaderUseCase(_loggerMock.Object, _checkServiceMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async System.Threading.Tasks.Task Execute_Should_Properly_Deserialize_Valid_Response()
        {
            // Arrange
            var responseData = new StatusValue
            {
                Status = CheckEligibilityStatus.eligible.ToString()
            };

            var initialResponse = new CheckEligibilityResponse
            {
                Data = responseData
            };

            var checkResponse = new CheckEligibilityStatusResponse
            {
                Data = new StatusValue { Status = "eligible" }
            };

            var responseJson = JsonConvert.SerializeObject(initialResponse);

            _checkServiceMock
                .Setup(x => x.GetStatus(It.IsAny<CheckEligibilityResponse>()))
                .ReturnsAsync(checkResponse);

            // Create claims to simulate a non-LA user
            var claims = CreateNonLAUserClaims();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // Act
            var result = await _sut.ExecuteAsync(responseJson, claimsPrincipal);
            var (viewName, model, status) = result;  // Deconstructing AdminLoaderResult

            // Assert
            status.Should().Be(CheckEligibilityStatus.eligible);
            viewName.Should().Be("Outcome/Eligible");
            model.Should().BeEquivalentTo(checkResponse.Data);

            _checkServiceMock.Verify(
                x => x.GetStatus(It.IsAny<CheckEligibilityResponse>()),
                Times.Once);
        }

        private List<Claim> CreateNonLAUserClaims()
        {
            var orgData = JsonConvert.SerializeObject(new
            {
                id = "123",
                name = "Test School",
                category = new { id = "1", name = "School" }
            });

            return new List<Claim>
            {
                new Claim("organisation", orgData)
            };
        }
    }
}
