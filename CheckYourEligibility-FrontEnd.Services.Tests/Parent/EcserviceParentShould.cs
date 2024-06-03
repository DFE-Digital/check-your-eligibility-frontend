using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility_FrontEnd.Services.Tests.Parent
{
    public class EcserviceParentShould
    {
        private IEcsServiceParent _sut;
        private ILogger<IEcsServiceParent> _loggerMock;
        private HttpClient _httpClient;
        private IConfiguration _config;

        [SetUp]
        public void Setup()
        {
            _loggerMock = Mock.Of<ILogger<IEcsServiceParent>>();
            _httpClient = new HttpClient();
            _sut = new EcsServiceParent(_loggerMock., _httpClient, _config);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }

        [Test]
        public void GetSchool()
        {
            // Arrange

            // Act
            //var result = _sut.GetSchool();

            // Assert
        }        
        
        [Test]
        public void PostApplication()
        {
            // Arrange

            // Act
            //var result = _sut.PostApplication();

            // Assert
        }  
        
        [Test]
        public void GetStatus()
        {
            // Arrange

            // Act
            //var result = _sut.GetStatus();

            // Assert
        }        
        [Test]
        public void PostCheck()
        {
            // Arrange

            // Act
            //var result = _sut.PostCheck();

            // Assert
        }
    }
}
