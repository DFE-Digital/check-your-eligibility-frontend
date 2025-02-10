using AutoFixture;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using CheckYourEligibility_FrontEnd.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminApplicationsRegisteredUseCaseTests
    {
        private Mock<ILogger<AdminApplicationsRegisteredUseCase>> _loggerMock;
        private Fixture _fixture;
        private AdminApplicationsRegisteredUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _loggerMock = new Mock<ILogger<AdminApplicationsRegisteredUseCase>>();
            _sut = new AdminApplicationsRegisteredUseCase(_loggerMock.Object);
        }

        [Test]
        public async Task Execute_WithValidJson_ReturnsSuccessResult()
        {
            // Arrange
            var viewModel = _fixture.Create<ApplicationConfirmationEntitledViewModel>();
            var applicationJson = JsonConvert.SerializeObject(viewModel);

            // Act
            var result = await _sut.Execute(applicationJson);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ViewModel.Should().BeEquivalentTo(viewModel);
            result.ErrorViewName.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithNullJson_ReturnsErrorResult()
        {
            // Arrange
            string applicationJson = null;

            // Act
            var result = await _sut.Execute(applicationJson);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorViewName.Should().Be("Outcome/Technical_Error");
            result.ViewModel.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithInvalidJson_ReturnsErrorResult()
        {
            // Arrange
            var applicationJson = "invalid json";

            // Act
            var result = await _sut.Execute(applicationJson);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorViewName.Should().Be("Outcome/Technical_Error");
            result.ViewModel.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithValidJsonButNullViewModel_InitializesChildrenList()
        {
            // Arrange
            var viewModel = _fixture.Build<ApplicationConfirmationEntitledViewModel>()
                .Without(x => x.Children)
                .Create();
            var applicationJson = JsonConvert.SerializeObject(viewModel);

            // Act
            var result = await _sut.Execute(applicationJson);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ViewModel.Children.Should().NotBeNull();
            result.ViewModel.Children.Should().BeEmpty();
        }
    }
}