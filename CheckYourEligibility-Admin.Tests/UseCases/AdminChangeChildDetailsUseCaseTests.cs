using AutoFixture;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminChangeChildDetailsUseCaseTests
    {
        private AdminChangeChildDetailsUseCase _sut;
        private Mock<ILogger<AdminChangeChildDetailsUseCase>> _loggerMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AdminChangeChildDetailsUseCase>>();
            _sut = new AdminChangeChildDetailsUseCase(_loggerMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void Execute_WithValidJson_ReturnsExpectedChildren()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    new Child
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Day = "01",
                        Month = "01",
                        Year = "2020"
                    }
                }
            };

            var fsmApplication = new FsmApplication { Children = children };
            var json = JsonConvert.SerializeObject(fsmApplication);

            // Act
            var result = _sut.Execute(json);

            // Assert
            result.Should().NotBeNull();
            result.ChildList.Should().NotBeNull();
            result.ChildList.Should().HaveCount(1);

            var firstChild = result.ChildList.First();
            firstChild.FirstName.Should().Be("John");
            firstChild.LastName.Should().Be("Doe");
            firstChild.Day.Should().Be("01");
            firstChild.Month.Should().Be("01");
            firstChild.Year.Should().Be("2020");
        }

        [Test]
        public void Execute_WithNullOrEmptyJson_ReturnsDefaultChildren()
        {
            // Act
            var result = _sut.Execute(null);

            // Assert
            result.Should().NotBeNull();
            result.ChildList.Should().NotBeNull();
            result.ChildList.Should().HaveCount(1);

            var defaultChild = result.ChildList.First();
            defaultChild.Should().NotBeNull();
            defaultChild.FirstName.Should().BeNull();
            defaultChild.LastName.Should().BeNull();
        }
    }
}