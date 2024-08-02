using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Idioms;
using CheckYourEligibility_FrontEnd.Controllers;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Security.Claims;
using CheckYourEligibility_DfeSignIn.Models;




namespace CheckYourEligibility.TestBase
{
    [ExcludeFromCodeCoverage]
    public abstract class TestBase
    {
        private ILogger<ApplicationController> _loggerMock;
        private Mock<IEcsServiceAdmin> _adminServiceMock;
        private Mock<ISession> _sessionMock;
        private Mock<HttpContext> _httpContext;
        private Mock<ClaimsPrincipal> _userMock;
        protected readonly Fixture _fixture = new Fixture();

        private ApplicationController _sut;

        [SetUp]
        public virtual void SetUp()
        {

            SetUpInitialMocks();
            SetUpSessionData();
            //SetUpClaimsData();
            SetUpHTTPContext();



            void SetUpInitialMocks()
            {
                _adminServiceMock = new Mock<IEcsServiceAdmin>();
                _loggerMock = Mock.Of<ILogger<ApplicationController>>();
                _sut = new ApplicationController(_loggerMock, _adminServiceMock.Object);

            };

            void SetUpSessionData()
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

            void SetUpHTTPContext()
            {
                _httpContext = new Mock<HttpContext>();
                _httpContext.Setup(ctx => ctx.Session).Returns(_sessionMock.Object);
                _sut.ControllerContext.HttpContext = _httpContext.Object;
            }

            //void SetUpClaimsData()
            //{
            //    _userMock = new Mock<ClaimsPrincipal>();
            //    var claimSchool = new Claim("organisation", Properties.Resources.ClaimSchool);
            //    _userMock.Setup(x => x.Claims).Returns(new List<Claim> { claimSchool,
            //        new Claim($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{ClaimConstants.NameIdentifier}", "123"),
            //        new Claim($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress","test@test.com"),
            //        new Claim($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname","testFirstName"),
            //        new Claim($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname","testSurname")
            //    });
            //}

        }


        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }


    }

}