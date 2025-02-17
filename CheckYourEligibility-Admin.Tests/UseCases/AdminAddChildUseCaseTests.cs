using AutoFixture;
using CheckYourEligibility.TestBase;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminAddChildUseCaseTests : TestBase
    {
        private Mock<ILogger<AdminAddChildUseCase>> _loggerMock;
        private AdminAddChildUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminAddChildUseCase>>();
            _sut = new AdminAddChildUseCase(_loggerMock.Object);
        }

        [Test]
        public void Execute_With_Null_Request_Should_Throw_ArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _sut.Execute(null));
            exception.ParamName.Should().Be("request");
        }

        [Test]
        public void Execute_With_Null_ChildList_Should_Initialize_And_Add_Child()
        {
            // Arrange
            var request = new Children { ChildList = null };

            // Act
            var result = _sut.Execute(request);

            // Assert
            result.ChildList.Should().NotBeNull();
            result.ChildList.Should().HaveCount(1);
            result.ChildList[0].Should().BeOfType<Child>();
        }

        [Test]
        public void Execute_Should_Add_Child_To_Existing_List()
        {
            // Arrange
            var request = new Children { ChildList = new List<Child> { new Child() } };
            var initialCount = request.ChildList.Count;

            // Act
            var result = _sut.Execute(request);

            // Assert
            result.ChildList.Should().HaveCount(initialCount + 1);
            result.ChildList.Last().Should().BeOfType<Child>();
        }
    }
}