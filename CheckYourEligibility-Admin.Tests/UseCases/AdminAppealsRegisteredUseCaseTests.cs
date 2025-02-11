using CheckYourEligibility_FrontEnd.UseCases.Admin;
using CheckYourEligibility_FrontEnd.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminAppealsRegisteredUseCaseTests
    {
        private Mock<ILogger<AdminAppealsRegisteredUseCase>> _loggerMock;
        private AdminAppealsRegisteredUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminAppealsRegisteredUseCase>>();
            _sut = new AdminAppealsRegisteredUseCase(_loggerMock.Object);
        }

        [Test]
        public async Task Execute_WithNullJson_ShouldReturnError()
        {
            // Arrange
            string? applicationJson = null;

            // Act
            var result = await _sut.Execute(applicationJson);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorViewName.Should().Be("Outcome/Technical_Error");
            result.ViewModel.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithEmptyJson_ShouldReturnError()
        {
            // Arrange
            var applicationJson = string.Empty;

            // Act
            var result = await _sut.Execute(applicationJson);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorViewName.Should().Be("Outcome/Technical_Error");
            result.ViewModel.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithInvalidJson_ShouldReturnError()
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
        public async Task Execute_WithValidJson_ShouldReturnSuccess()
        {
            // Arrange
            var viewModel = new ApplicationConfirmationEntitledViewModel
            {
                ParentName = "John Smith",
                Children = new List<ApplicationConfirmationEntitledChildViewModel>()
            };
            var applicationJson = JsonConvert.SerializeObject(viewModel);

            // Act
            var result = await _sut.Execute(applicationJson);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ViewModel.Should().NotBeNull();
            result.ViewModel.ParentName.Should().Be("John Smith");
            result.ViewModel.Children.Should().BeEmpty();
        }

        [Test]
        public async Task Execute_WithMissingProperties_ShouldInitializeEmptyCollections()
        {
            // Arrange
            var viewModel = new ApplicationConfirmationEntitledViewModel
            {
                ParentName = null,
                Children = null
            };
            var applicationJson = JsonConvert.SerializeObject(viewModel);

            // Act
            var result = await _sut.Execute(applicationJson);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ViewModel.Should().NotBeNull();
            result.ViewModel.ParentName.Should().BeNull();
            result.ViewModel.Children.Should().NotBeNull();
            result.ViewModel.Children.Should().BeEmpty();
        }

        [Test]
        public async Task Execute_ShouldLogInformation()
        {
            // Arrange
            var viewModel = new ApplicationConfirmationEntitledViewModel
            {
                ParentName = "John Smith"
            };
            var applicationJson = JsonConvert.SerializeObject(viewModel);

            // Act
            await _sut.Execute(applicationJson);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Successfully processed admin appeal")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}