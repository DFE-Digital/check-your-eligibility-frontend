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
    public class BulkUploadTests
    {
        // mocks
        private ILogger<BulkUploadController> _loggerMock;
       // private Mock<IEcsServiceParent> _parentServiceMock;
        private Mock<IEcsCheckService> _checkServiceMock;
        private Mock<ISession> _sessionMock;
        private Mock<HttpContext> _httpContext;
        private Mock<IConfiguration> _configMock;

        // check responses
        // check eligibility responses
        private CheckEligibilityResponseBulk _checkEligibilityResponse;

        // test data entities
        
        // system under test
        private BulkUploadController _sut;

        [SetUp]
        public void SetUp()
        {
            //SetUpTestData();
            SetUpInitialMocks();
            SetUpTempData();
            //SetUpSessionData();
            //SetUpHTTPContext();
            SetUpServiceMocks();

            void SetUpInitialMocks()
            {
                _configMock = new Mock<IConfiguration>();
                //_parentServiceMock = new Mock<IEcsServiceParent>();
                _checkServiceMock = new Mock<IEcsCheckService>();
                _loggerMock = Mock.Of<ILogger<BulkUploadController>>();
                _sut = new BulkUploadController(_loggerMock,  _checkServiceMock.Object, _configMock.Object);
            }

            void SetUpServiceMocks()
            {
                _checkServiceMock.Setup(s => s.PostBulkCheck(It.IsAny<CheckEligibilityRequestBulk>()))
                    .ReturnsAsync(_checkEligibilityResponse);
            }
        }
        void SetUpTempData()
        {
            ITempDataProvider tempDataProvider = Mock.Of<ITempDataProvider>();
            TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
            ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());
            _sut.TempData = tempData;
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }


        [Test]
        public async Task Given_Batch_Check_Should_Load_BatchCheckPage()
        {
            // Act
            var result = _sut.Batch_Check();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeNull();
        }

        //[Test]
        //public async Task Given_Batch_Check_When_FileData_Invalid_Should_Return_BadRequest()
        //{
        //    // Arrange
        //    var content = Properties.Resources.batchchecktemplate_some_invalid_items;
        //    var fileName = "test.csv";
        //    var stream = new MemoryStream();
        //    var writer = new StreamWriter(stream);
        //    writer.Write(content);
        //    writer.Flush();
        //    stream.Position = 0;

        //    //create FormFile with desired data
        //    var file = new FormFile(stream, 0, stream.Length, fileName, fileName)
        //    {
        //        Headers = new HeaderDictionary(),
        //        ContentType = "text/csv"
        //    };//_sut.TempData["ParentDetails"] = JsonConvert.SerializeObject(_parent);

        //    // Act
        //    var result = _sut.Batch_Check(file);

        //    // Assert
        //    //result.Should().BeOfType<ViewResult>();
        //    //var viewResult = result as ViewResult;
        //    //viewResult.Model.Should().BeOfType<Parent>();
        //    //var model = viewResult.Model as Parent;
        //    //model.Should().BeEquivalentTo(_parent);
        //}

    }
}

