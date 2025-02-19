using AutoFixture;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class AdminEnterChildDetailsUseCaseTests
    {
        private AdminEnterChildDetailsUseCase _sut;
        private Mock<ILogger<AdminEnterChildDetailsUseCase>> _loggerMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminEnterChildDetailsUseCase>>();
            _sut = new AdminEnterChildDetailsUseCase(_loggerMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Execute_InitialLoad_ReturnsDefaultModel()
        {
            // Act
            var result = await _sut.Execute();

            // Assert
            result.Should().NotBeNull();
            result.Children.Should().NotBeNull();
            result.Children.ChildList.Should().HaveCount(1);
            result.Children.ChildList.First().Should().BeOfType<Child>();
            result.IsRedirect.Should().BeFalse();
            result.ModelState.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithChildAddOrRemove_ReturnsUpdatedModel()
        {
            // Arrange
            var childList = _fixture.CreateMany<Child>(2).ToList();
            var childListJson = JsonConvert.SerializeObject(childList);

            // Act
            var result = await _sut.Execute(isChildAddOrRemove: true, childListJson: childListJson);

            // Assert
            result.Should().NotBeNull();
            result.Children.Should().NotBeNull();
            result.Children.ChildList.Should().HaveCount(2);
            result.IsRedirect.Should().BeFalse();
            result.ModelState.Should().NotBeNull();
        }

        [Test]
        public async Task Execute_WithChildAddOrRemove_InvalidJson_ReturnsDefaultModel()
        {
            // Arrange
            const string invalidJson = "invalid json";

            // Act
            var result = await _sut.Execute(isChildAddOrRemove: true, childListJson: invalidJson);

            // Assert
            result.Should().NotBeNull();
            result.Children.Should().NotBeNull();
            result.Children.ChildList.Should().HaveCount(1);
            result.Children.ChildList.First().Should().BeOfType<Child>();
            result.IsRedirect.Should().BeFalse();
            result.ModelState.Should().NotBeNull();

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deserializing child list JSON")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task Execute_WithRedirectFromFsmApplication_ReturnsModelWithChildren()
        {
            // Arrange
            var fsmApplication = _fixture.Create<FsmApplication>();
            var fsmApplicationJson = JsonConvert.SerializeObject(fsmApplication);

            // Act
            var result = await _sut.Execute(
                isChildAddOrRemove: false,
                childListJson: null,
                fsmApplicationJson: fsmApplicationJson,
                isRedirect: true);

            // Assert
            result.Should().NotBeNull();
            result.Children.Should().NotBeNull();
            result.Children.Should().BeEquivalentTo(fsmApplication.Children);
            result.IsRedirect.Should().BeTrue();
            result.ModelState.Should().BeNull();
        }

        [Test]
        public async Task Execute_WithRedirectFromFsmApplication_InvalidJson_ReturnsDefaultModel()
        {
            // Arrange
            const string invalidJson = "invalid json";

            // Act
            var result = await _sut.Execute(
                isChildAddOrRemove: false,
                childListJson: null,
                fsmApplicationJson: invalidJson,
                isRedirect: true);

            // Assert
            result.Should().NotBeNull();
            result.Children.Should().NotBeNull();
            result.Children.ChildList.Should().HaveCount(1);
            result.Children.ChildList.First().Should().BeOfType<Child>();
            result.IsRedirect.Should().BeTrue();
            result.ModelState.Should().BeNull();

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deserializing FSM application JSON")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task Execute_WithRedirectFromFsmApplication_NullChildren_ReturnsDefaultModel()
        {
            // Arrange
            var fsmApplication = _fixture.Create<FsmApplication>();
            fsmApplication.Children = null;
            var fsmApplicationJson = JsonConvert.SerializeObject(fsmApplication);

            // Act
            var result = await _sut.Execute(
                isChildAddOrRemove: false,
                childListJson: null,
                fsmApplicationJson: fsmApplicationJson,
                isRedirect: true);

            // Assert
            result.Should().NotBeNull();
            result.Children.Should().NotBeNull();
            result.Children.ChildList.Should().HaveCount(1);
            result.Children.ChildList.First().Should().BeOfType<Child>();
            result.IsRedirect.Should().BeTrue();
            result.ModelState.Should().BeNull();

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("FSM application or children were null")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}