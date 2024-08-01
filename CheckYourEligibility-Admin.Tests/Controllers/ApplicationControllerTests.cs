using CheckYourEligibility_FrontEnd.Controllers;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.Configuration;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Idioms;
using CheckYourEligibility_FrontEnd.Models;

namespace CheckYourEligibility_Parent.Tests.Controllers
{
    [TestFixture]
    public class ApplicationControllerTests
    {
        // mocks
        private ILogger<ApplicationController> _loggerMock;
        private Mock<IEcsServiceAdmin> _adminServiceMock;
        private Mock<ISession> _sessionMock;
        private Mock<HttpContext> _httpContext;
        protected readonly Fixture _fixture = new Fixture();

        //private Mock<IConfiguration> _configMock;

        // responses
        //private ApplicationSearchResponse _applicationSearchResponse;
        //private ApplicationResponse _applicationResponse;

        //system under test
        private ApplicationController _sut;

        [SetUp]
        public void SetUp()
        {

            SetUpInitialMocks();
            // SetUpServiceMocks();

            void SetUpInitialMocks()
            {
                _adminServiceMock = new Mock<IEcsServiceAdmin>();
                _loggerMock = Mock.Of<ILogger<ApplicationController>>();
                _sut = new ApplicationController(_loggerMock, _adminServiceMock.Object);

            };

            //void SetUpServiceMocks()
            //{
            //    _checkServiceMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
            //        .ReturnsAsync(_applicationSearchResponse);
            //}
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public async Task Given_Application_Search_Should_Load_ApplicationSearchPage()
        {
            // Act
            var result = _sut.Search();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public async Task Given_Application_Search_Results_Page_Returns_Valid_Data()
        {
            //arrange
            var response = _fixture.Create<ApplicationSearchResponse>();

            _adminServiceMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
                   .ReturnsAsync(response);

            var request = _fixture.Create<ApplicationSearch>();


            //act
            var result = await _sut.Results(request);

            //assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeAssignableTo<ApplicationSearchResponse>();

            var model = viewResult.Model as ApplicationSearchResponse;
            model.Should().NotBeNull();
            model.Should().BeEquivalentTo(_adminServiceMock.Object);

        }
    }

}
