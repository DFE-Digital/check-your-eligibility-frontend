using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Newtonsoft.Json;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminLoaderUseCaseTests : IDisposable
    {
        private Mock<ILogger<AdminLoaderUseCase>> _loggerMock;
        private Mock<IEcsCheckService> _checkServiceMock;
        private AdminLoaderUseCase _sut;
        private List<Claim> _claims;
        private bool _disposed;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminLoaderUseCase>>();
            _checkServiceMock = new Mock<IEcsCheckService>();
            _sut = new AdminLoaderUseCase(_loggerMock.Object, _checkServiceMock.Object);

            _claims = new List<Claim>
            {
                new Claim("organisation", "LocalAuthority")
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _sut = null;
            }

            _disposed = true;
        }

        [Test]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            FluentActions.Invoking(() => new AdminLoaderUseCase(null, _checkServiceMock.Object))
                .Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Test]
        public void Constructor_WithNullCheckService_ThrowsArgumentNullException()
        {
            // Act & Assert
            FluentActions.Invoking(() => new AdminLoaderUseCase(_loggerMock.Object, null))
                .Should().Throw<ArgumentNullException>()
                .WithParameterName("checkService");
        }

        [Test]
        public async Task Execute_WithNullJson_ReturnsTechnicalError()
        {
            // Act
            var result = await _sut.Execute(null, _claims);

            // Assert
            result.ViewName.Should().Be("Outcome/Technical_Error");
            result.Model.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithEmptyJson_ReturnsTechnicalError()
        {
            // Act
            var result = await _sut.Execute("", _claims);

            // Assert
            result.ViewName.Should().Be("Outcome/Technical_Error");
            result.Model.Should().BeNull();
        }

        [Test]
        public async Task Execute_WhenCheckServiceReturnsNullResponse_ReturnsTechnicalError()
        {
            // Arrange
            var response = new CheckEligibilityResponse();
            var json = JsonConvert.SerializeObject(response);

            _checkServiceMock.Setup(x => x.GetStatus(It.IsAny<CheckEligibilityResponse>()))
                .ReturnsAsync((CheckEligibilityStatusResponse)null);

            // Act
            var result = await _sut.Execute(json, _claims);

            // Assert
            result.ViewName.Should().Be("Outcome/Technical_Error");
            result.Model.Should().BeNull();
        }

        [TestCase(CheckEligibilityStatus.eligible, "Outcome/Eligible_LA", true)]
        [TestCase(CheckEligibilityStatus.eligible, "Outcome/Eligible", false)]
        [TestCase(CheckEligibilityStatus.notEligible, "Outcome/Not_Eligible_LA", true)]
        [TestCase(CheckEligibilityStatus.notEligible, "Outcome/Not_Eligible", false)]
        [TestCase(CheckEligibilityStatus.parentNotFound, "Outcome/Not_Found", true)]
        [TestCase(CheckEligibilityStatus.DwpError, "Outcome/Technical_Error", true)]
        [TestCase(CheckEligibilityStatus.queuedForProcessing, "Loader", true)]
        public async Task Execute_WithStatus_ReturnsCorrectView(
            CheckEligibilityStatus status,
            string expectedView,
            bool isLocalAuthority)
        {
            // Arrange
            var response = new CheckEligibilityResponse();
            var json = JsonConvert.SerializeObject(response);
            var claims = new List<Claim>
            {
                new Claim("organisation", isLocalAuthority ? "LocalAuthority" : "other")
            };

            _checkServiceMock.Setup(x => x.GetStatus(It.IsAny<CheckEligibilityResponse>()))
                .ReturnsAsync(new CheckEligibilityStatusResponse
                {
                    Data = new StatusValue
                    {
                        Status = status.ToString()
                    }
                });

            // Act
            var result = await _sut.Execute(json, claims);

            // Assert
            result.ViewName.Should().Be(expectedView);
            result.Model.Should().BeNull();
        }

        [Test]
        public async Task Execute_WhenServiceThrows_ThrowsAdminLoaderException()
        {
            // Arrange
            var response = new CheckEligibilityResponse();
            var json = JsonConvert.SerializeObject(response);

            _checkServiceMock.Setup(x => x.GetStatus(It.IsAny<CheckEligibilityResponse>()))
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await FluentActions.Invoking(() => _sut.Execute(json, _claims))
                .Should().ThrowAsync<AdminLoaderException>()
                .WithMessage("Failed to process eligibility check status: Service error");

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Failed to process eligibility check status")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task Execute_WhenCheckServiceReturnsNullData_ReturnsTechnicalError()
        {
            // Arrange
            var response = new CheckEligibilityResponse();
            var json = JsonConvert.SerializeObject(response);

            _checkServiceMock.Setup(x => x.GetStatus(It.IsAny<CheckEligibilityResponse>()))
                .ReturnsAsync(new CheckEligibilityStatusResponse { Data = null });

            // Act
            var result = await _sut.Execute(json, _claims);

            // Assert
            result.ViewName.Should().Be("Outcome/Technical_Error");
            result.Model.Should().BeNull();
        }
    }
}