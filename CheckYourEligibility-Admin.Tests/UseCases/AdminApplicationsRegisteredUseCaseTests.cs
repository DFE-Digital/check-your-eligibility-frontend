using AutoFixture;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using CheckYourEligibility_FrontEnd.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class AdminApplicationsRegisteredUseCaseTests
    {
        private AdminApplicationsRegisteredUseCase _sut;
        private Mock<ILogger<AdminApplicationsRegisteredUseCase>> _loggerMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminApplicationsRegisteredUseCase>>();
            _sut = new AdminApplicationsRegisteredUseCase(_loggerMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Execute_WithValidJson_ReturnsSuccessResult()
        {
            // Arrange
            var viewModel = _fixture.Create<ApplicationConfirmationEntitledViewModel>();
            var json = JsonConvert.SerializeObject(viewModel);

            // Act
            var result = await _sut.Execute(json);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ViewModel.Should().BeEquivalentTo(viewModel);
            result.ErrorViewName.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithNullJson_ReturnsErrorResult()
        {
            // Act
            var result = await _sut.Execute(null);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorViewName.Should().Be("Outcome/Technical_Error");
            result.ViewModel.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithEmptyJson_ReturnsErrorResult()
        {
            // Act
            var result = await _sut.Execute(string.Empty);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorViewName.Should().Be("Outcome/Technical_Error");
            result.ViewModel.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithInvalidJson_ReturnsErrorResult()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act
            var result = await _sut.Execute(invalidJson);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorViewName.Should().Be("Outcome/Technical_Error");
            result.ViewModel.Should().BeNull();
        }
    }
}