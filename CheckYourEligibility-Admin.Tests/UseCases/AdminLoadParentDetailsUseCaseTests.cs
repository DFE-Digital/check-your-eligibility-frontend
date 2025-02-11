using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminLoadParentDetailsUseCaseTests
    {
        private Mock<ILogger<AdminLoadParentDetailsUseCase>> _loggerMock;
        private AdminLoadParentDetailsUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminLoadParentDetailsUseCase>>();
            _sut = new AdminLoadParentDetailsUseCase(_loggerMock.Object);
        }

        [Test]
        public async Task Execute_WithNoData_ShouldReturnNulls()
        {
            // Act
            var (parent, errors) = await _sut.Execute();

            // Assert
            parent.Should().BeNull();
            errors.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithValidParentJson_ShouldDeserializeParent()
        {
            // Arrange
            var expectedParent = new ParentGuardian
            {
                FirstName = "Test",
                LastName = "Parent",
                EmailAddress = "test@example.com"
            };
            var parentJson = JsonConvert.SerializeObject(expectedParent);

            // Act
            var (parent, errors) = await _sut.Execute(parentJson);

            // Assert
            parent.Should().BeEquivalentTo(expectedParent);
            errors.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithValidErrorsJson_ShouldDeserializeErrors()
        {
            // Arrange
            var expectedErrors = new Dictionary<string, List<string>>
            {
                { "Field1", new List<string> { "Error1" } }
            };
            var errorsJson = JsonConvert.SerializeObject(expectedErrors);

            // Act
            var (parent, errors) = await _sut.Execute(null, errorsJson);

            // Assert
            parent.Should().BeNull();
            errors.Should().BeEquivalentTo(expectedErrors);
        }

        [Test]
        public async Task Execute_WithInvalidParentJson_ShouldReturnNull()
        {
            // Arrange
            var invalidJson = "invalid json";

            // Act
            var (parent, errors) = await _sut.Execute(invalidJson);

            // Assert
            parent.Should().BeNull();
            errors.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithNinoNassValidation_ShouldProcessSpecialCase()
        {
            // Arrange
            var validationErrors = new Dictionary<string, List<string>>
            {
                { "NationalInsuranceNumber", new List<string> { "Please select one option" } },
                { "NationalAsylumSeekerServiceNumber", new List<string> { "Please select one option" } }
            };
            var errorsJson = JsonConvert.SerializeObject(validationErrors);

            // Act
            var (_, errors) = await _sut.Execute(null, errorsJson);

            // Assert
            errors.Should().NotContainKey("NationalInsuranceNumber");
            errors.Should().NotContainKey("NationalAsylumSeekerServiceNumber");
            errors.Should().ContainKey("NINAS");
            errors["NINAS"].Should().Contain("Please select one option");
        }

        [Test]
        public async Task Execute_WithInvalidErrorsJson_ShouldReturnNullErrors()
        {
            // Arrange
            var invalidJson = "invalid json";

            // Act
            var (parent, errors) = await _sut.Execute(null, invalidJson);

            // Assert
            parent.Should().BeNull();
            errors.Should().BeNull();
        }
    }
}