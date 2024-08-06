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
using System.Security.Principal;
using System.Security.Claims;
using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility.TestBase;

namespace CheckYourEligibility_Parent.Tests.Controllers
{
    [TestFixture]
    public class ApplicationControllerTests : TestBase
    {
        // mocks
        private ILogger<ApplicationController> _loggerMock;
        private Mock<IEcsServiceAdmin> _adminServiceMock;
        private Mock<ISession> _sessionMock;
        private Mock<HttpContext> _httpContext;
        private Mock<ClaimsPrincipal> _userMock;
        


        private ApplicationController _sut;


        [SetUp]
        public void SetUp()
        {
            SetUpInitialMocks();
            _sut = new ApplicationController(_loggerMock, _adminServiceMock.Object);
            SetUpSessionData();
            SetUpClaimsData();
            SetUpHTTPContext();

        }

        public void SetUpInitialMocks()
        {
            _adminServiceMock = new Mock<IEcsServiceAdmin>();
            _loggerMock = Mock.Of<ILogger<ApplicationController>>();
        }

        public void SetUpHTTPContext()
        {
            _httpContext = new Mock<HttpContext>();
            _httpContext.Setup(ctx => ctx.Session).Returns(_sessionMock.Object);
            _httpContext.Setup(ctx => ctx.User).Returns(_userMock.Object);
            _sut.ControllerContext.HttpContext = _httpContext.Object;
        }

        public void SetUpSessionData()
        {
            _sessionMock = new Mock<ISession>();
            var sessionStorage = new Dictionary<string, byte[]>();

            _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                            .Callback<string, byte[]>((key, value) => sessionStorage[key] = value);

            _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                        .Returns((string key, out byte[] value) =>
                        {
                            var result = sessionStorage.TryGetValue(key, out var storedValue);
                            value = storedValue;
                            return result;
                        });
        }

        void SetUpClaimsData()
        {
            _userMock = new Mock<ClaimsPrincipal>();
            var claimSchool = new Claim("organisation", Properties.Resources.ClaimSchool);
            _userMock.Setup(x => x.Claims).Returns(new List<Claim> { claimSchool,
                    new Claim($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{ClaimConstants.NameIdentifier}", "123"),
                    new Claim($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress","test@test.com"),
                    new Claim($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname","testFirstName"),
                    new Claim($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname","testSurname")
                });
        }

        public void SetUpTempData()
        {
            var mockTempDataProvider = new Mock<ITempDataProvider>();
            var mockTempDataDict = new TempDataDictionary(_httpContext.Object, mockTempDataProvider.Object);
            _sut.TempData = mockTempDataDict;
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public async Task Given_Application_Search_Should_Load_ApplicationSearchPage()
        {
            // Arrange 
            SetUpTempData();
            // Act
            var result = _sut.Search();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public async Task Given_Application_Search_Returns_No_Records_User_Redirected_To_Search()
        {
            //Arrange
            SetUpTempData();
            var response = new ApplicationSearchResponse();

            _adminServiceMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
                .ReturnsAsync(response);

            var request = new ApplicationSearch();

            //act
            var result = await _sut.Results(request);

            //assert 

            var Result = result.Should().BeOfType<RedirectToActionResult>().Subject;
            Result.ActionName.Should().Be("Search");
            _sut.TempData["Message"].Should().Be("There are no records matching your search.");
        }

        
        [Test]
        public async Task Given_Application_Search_Results_Page_Returns_Valid_Data()
        {
            //arrange
            var response = _fixture.Create<ApplicationSearchResponse>();

            _adminServiceMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
                   .ReturnsAsync(response);

            var request = new ApplicationSearch();

            //act
            var result = await _sut.Results(request);

            //assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeAssignableTo<ApplicationSearchResponse>();

            var model = viewResult.Model as ApplicationSearchResponse;
            model.Should().NotBeNull();
            model.Should().BeEquivalentTo(response);

        }
    }

}
