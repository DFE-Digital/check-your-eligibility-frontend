using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases;
using FluentAssertions;
using Moq;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    public class SearchSchoolsUseCaseTests
    {
        private Mock<IEcsServiceParent> _parentServiceMock;
        private SearchSchoolsUseCase _sut;

        [SetUp]
        public void Setup()
        {
            _parentServiceMock = new Mock<IEcsServiceParent>();
            _sut = new SearchSchoolsUseCase(_parentServiceMock.Object);
        }

        [Test]
        public async Task ExecuteAsync_WithValidQuery_ReturnsSchools()
        {
            // Arrange
            var query = "Test School";
            var schoolResponse = new EstablishmentSearchResponse
            {
                Data = new List<Establishment>
                {
                    new() { Name = "Test School", Postcode = "TE1 1ST" }
                }
            };
            _parentServiceMock
                .Setup(x => x.GetSchool(query))
                .ReturnsAsync(schoolResponse);

            // Act
            var results = await _sut.ExecuteAsync(query);

            // Assert
            results.Should().NotBeEmpty();
            results.First().Name.Should().Be("Test School");
            results.First().Postcode.Should().Be("TE1 1ST");
        }

        [Test]
        public async Task ExecuteAsync_WithShortQuery_ThrowsArgumentException()
        {
            // Act & Assert
            await FluentActions
                .Invoking(() => _sut.ExecuteAsync("ab"))
                .Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Query must be at least 3 characters long.*");
        }

        [Test]
        public async Task ExecuteAsync_WithNullQuery_ThrowsArgumentException()
        {
            // Act & Assert
            await FluentActions
                .Invoking(() => _sut.ExecuteAsync(null))
                .Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Query must be at least 3 characters long.*");
        }

        [Test]
        public async Task ExecuteAsync_WhenNoSchoolsFound_ReturnsEmptyList()
        {
            // Arrange
            var query = "NonExistent School";
            _parentServiceMock
                .Setup(x => x.GetSchool(query))
                .ReturnsAsync(new EstablishmentSearchResponse { Data = new List<Establishment>() });

            // Act
            var results = await _sut.ExecuteAsync(query);

            // Assert
            results.Should().BeEmpty();
        }
    }
}