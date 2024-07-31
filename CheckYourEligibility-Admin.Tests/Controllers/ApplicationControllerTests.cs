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

namespace CheckYourEligibility_Parent.Tests.Controllers
{
    [TestFixture]
    public class ApplicationControllerTests
    {
        // mocks
        private ILogger<ApplicationController> _loggerMock;
        private Mock<IEcsServiceAdmin> _checkServiceMock;
        private Mock<ISession> _sessionMock;
        private Mock<HttpContext> _httpContext;
        //private Mock<IConfiguration> _configMock;

        // responses
        private ApplicationSearchResponse _applicationSearchResponse;

        //system under test
        private ApplicationController _sut;

        [SetUp]
        public void SetUp()
        {
            SetUpInitialMocks();
            SetUpServiceMocks();

            void SetUpInitialMocks()
            {
                //_configMock = new Mock<IConfiguration>();
                _checkServiceMock = new Mock<IEcsServiceAdmin>();
                _loggerMock = Mock.Of<ILogger<ApplicationController>>();
                _sut = new ApplicationController(_loggerMock, _checkServiceMock.Object);

            }

            void SetUpServiceMocks()
            {
                _checkServiceMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
                    .ReturnsAsync(_applicationSearchResponse);
            }
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public async Task Given_Application_Search_Should_Load_ApplicationSearchPage()
        {
            var result = _sut.Search();

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeNull();
        }
    }

}
