using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Idioms;
using Moq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace CheckYourEligibility.TestBase
{
    [ExcludeFromCodeCoverage]
    public abstract class TestBase
    {
        private ILogger<ApplicationController> _loggerMock;
        private Mock<IEcsServiceAdmin> _adminServiceMock;
        private Mock<ISession> _sessionMock;
        private Mock<HttpContext> _httpContext;
        protected readonly Fixture _fixture = new Fixture();


        [SetUp]
        public void SetUp()
        {

            SetUpInitialMocks();
            SetUpHTTPContext();


            void SetUpInitialMocks()
            {
                _adminServiceMock = new Mock<IEcsServiceAdmin>();
                _loggerMock = Mock.Of<ILogger<ApplicationController>>();
                _sut = new ApplicationController(_loggerMock, _adminServiceMock.Object);

            };

            void SetUpHTTPContext()
            {
                _httpContext = new Mock<HttpContext>();
                _httpContext.Setup(ctx => ctx.Session).Returns(_sessionMock.Object);
                _sut.ControllerContext.HttpContext = _httpContext.Object;
            }

        }
        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }


    }
}