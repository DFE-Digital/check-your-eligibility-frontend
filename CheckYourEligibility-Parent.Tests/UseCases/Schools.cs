using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases.Schools.GetSchoolDetailsUseCase;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;



namespace CheckYourEligibility_Parent.Tests.UseCases.Schools
{
    [TestFixture]
    public class GetSchoolDetailsUseCaseTests
    {
        private Mock<IEcsServiceParent> _parentServiceMock;
        private Mock<ILogger<GetSchoolDetailsUseCase>> _loggerMock;
        private GetSchoolDetailsUseCase _sut;

        [SetUp]
        public void Setup()
        {
            _parentServiceMock = new Mock<IEcsServiceParent>();
            _loggerMock = new Mock<ILogger<GetSchoolDetailsUseCase>>();
            _sut = new GetSchoolDetailsUseCase(_parentServiceMock.Object, _loggerMock.Object);
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
            var result = await _sut.ExecuteAsync(new GetSchoolDetailsRequest(query));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Schools.Should().NotBeEmpty();
            result.Schools.First().Name.Should().Be("Test School");
        }

        [Test]
        public async Task ExecuteAsync_WithShortQuery_ReturnsFailure()
        {
            // Arrange
            var query = "ab";

            // Act
            var result = await _sut.ExecuteAsync(new GetSchoolDetailsRequest(query));

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Query must be at least 3 characters long.");
        }
    }
}
