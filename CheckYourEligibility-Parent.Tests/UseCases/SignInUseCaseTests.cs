using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class SignInUseCaseTests
    {
        private SignInUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new SignInUseCase();
        }

        [Test]
        public async Task Execute_ShouldReturnAuthenticationPropertiesWithCorrectValues()
        {
            // Arrange
            var redirectUri = "/Test/Redirect";

            // Act
            var result = await _sut.Execute(redirectUri);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<AuthenticationProperties>();
            result.RedirectUri.Should().Be(redirectUri);

            // Get items dictionary directly as GetString extension method is not working in tests
            var vectorOfTrust = result.Items["vector_of_trust"];
            vectorOfTrust.Should().Be(@"[""Cl""]");
        }

        [Test]
        public async Task Execute_WithNullRedirectUri_ShouldStillCreateValidProperties()
        {
            // Arrange
            string redirectUri = null;

            // Act
            var result = await _sut.Execute(redirectUri);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<AuthenticationProperties>();
            result.RedirectUri.Should().BeNull();

            // Get items dictionary directly as GetString extension method is not working in tests
            var vectorOfTrust = result.Items["vector_of_trust"];
            vectorOfTrust.Should().Be(@"[""Cl""]");
        }
    }
}