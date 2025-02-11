using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace CheckYourEligibility_Admin.Tests.UseCases
{
    [TestFixture]
    public class AdminSearchSchoolsUseCaseTests
    {
        private Mock<IEcsServiceParent> _parentServiceMock;
        private AdminSearchSchoolsUseCase _sut;

        [SetUp]
        public void Setup()
        {
            _parentServiceMock = new Mock<IEcsServiceParent>();
            _sut = new AdminSearchSchoolsUseCase(_parentServiceMock.Object);
        }

        [Test]
        public void Constructor_WhenParentServiceIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            FluentActions.Invoking(() => new AdminSearchSchoolsUseCase(null))
                .Should().Throw<ArgumentNullException>()
                .WithParameterName("parentService");
        }

        [TestCaseSource(nameof(ShortQueries))]
        public async Task Execute_WhenQueryIsTooShort_ShouldThrowArgumentException(string query)
        {
            // Act & Assert
            Func<Task> act = () => _sut.Execute(query);
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Query must be at least 3 characters long.*");
        }

        private static IEnumerable<string> ShortQueries()
        {
            yield return "";
            yield return null;
            yield return "ab";
        }

        [Test]
        public async Task Execute_WhenValidQuery_ShouldReturnSchools()
        {
            // Arrange
            var query = "Test School";
            var expectedResults = new EstablishmentSearchResponse
            {
                Data = new List<Establishment>
                {
                    new() { Name = "Test School 1" },
                    new() { Name = "Test School 2" }
                }
            };

            _parentServiceMock.Setup(x => x.GetSchool(query))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _sut.Execute(query);

            // Assert
            result.Should().BeEquivalentTo(expectedResults.Data);
            _parentServiceMock.Verify(x => x.GetSchool(query), Times.Once);
        }

        [Test]
        public async Task Execute_WhenServiceReturnsNull_ShouldReturnEmptyList()
        {
            // Arrange
            var query = "Test School";
            _parentServiceMock.Setup(x => x.GetSchool(query))
                .ReturnsAsync((EstablishmentSearchResponse)null);

            // Act
            var result = await _sut.Execute(query);

            // Assert
            result.Should().BeEmpty();
            _parentServiceMock.Verify(x => x.GetSchool(query), Times.Once);
        }
    }
}