using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.TestBase;
using CheckYourEligibility_FrontEnd.Controllers;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_Parent.Tests.Properties;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility_Admin.Tests.Controllers
{
    [TestFixture]
    public class BulkUploadTests : TestBase
    {
        // mocks
        private ILogger<BulkUploadController> _loggerMock;
        private Mock<IEcsCheckService> _checkServiceMock;

        // system under test
        private BulkUploadController _sut;

        [SetUp]
        public void SetUp()
        {
            _checkServiceMock = new Mock<IEcsCheckService>();
            _loggerMock = Mock.Of<ILogger<BulkUploadController>>();
            _sut = new BulkUploadController(_loggerMock, _checkServiceMock.Object, _configMock.Object);
      
            base.SetUp();

            _sut.TempData = _tempData;
            _sut.ControllerContext.HttpContext = _httpContext.Object;
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

        [Test]
        public async Task Given_Batch_Check_When_FileData_Invalid_Should_Return_Error_Data_Issue()
        {
            // Arrange
            var content = Resources.batchchecktemplate_some_invalid_items;
            var fileName = "test.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            //create FormFile with desired data
            var file = new FormFile(stream, 0, stream.Length, fileName, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/csv"
            };

            // Act
            var result = await _sut.Batch_Check(file);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().BeEquivalentTo("BatchOutcome/Error_Data_Issue");
            viewResult.TempData["BatchParentCheckItemsErrors"].Should().NotBeNull();
        }

        [Test]
        public async Task Given_Batch_Check_When_FileData_Empty_Should_Return_Error_Data_Issue()
        {
            // Arrange
            var content = "";
            var fileName = "test.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            //create FormFile with desired data
            var file = new FormFile(stream, 0, stream.Length, fileName, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/csv"
            };

            // Act
            var result = await _sut.Batch_Check(file);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().BeEquivalentTo("BatchOutcome/Error_Data_Issue");
            viewResult.TempData["BatchParentCheckItemsErrors"].Should().BeEquivalentTo("Invalid file content.\r\n");
        }

        [Test]
        public async Task Given_Batch_Check_When_FileType_Invalid_Should_Return_RedirectToActionResult()
        {
            // Arrange
            var content = "";
            var fileName = "test.xls";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            //create FormFile with desired data
            var file = new FormFile(stream, 0, stream.Length, fileName, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/xls"
            };

            // Act
            var result = await _sut.Batch_Check(file);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
        }

        [Test]
        public async Task Given_Batch_Check_When_FileData_Valid_Should_Return_ValidData()
        {
            // Arrange
            var response =
                new CheckEligibilityResponseBulk
                {
                    Data = new StatusValue { Status = "processing" },
                    Links = new CheckEligibilityResponseBulkLinks { Get_BulkCheck_Results = "someUrl", Get_Progress_Check = "someUrl" }
                };
            _checkServiceMock.Setup(s => s.PostBulkCheck(It.IsAny<CheckEligibilityRequestBulk>()))
                    .ReturnsAsync(response);

            var content = Resources.batchchecktemplate_small_Valid;
            var fileName = "test.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            //create FormFile with desired data
            var file = new FormFile(stream, 0, stream.Length, fileName, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/csv"
            };

            // Act
            var result = await _sut.Batch_Check(file);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;
            viewResult.ActionName.Should().BeEquivalentTo("Batch_Loader");

        }


        [Test]
        public async Task Given_Loader_When_LoadingPage_Should_return_LoadLoaderPage()
        {
            // Act
            var result = await _sut.Batch_Loader();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public async Task Given_Loader_When_Results_Should_return_redirect()
        {
            //Arrange
            //  HttpContext.Session.SetString("Get_Progress_Check", result.Links.Get_Progress_Check);
            var response =
               new CheckEligibilityBulkStatusResponse { Data = new BulkStatus { Complete = 10, Total = 10 } };

            _checkServiceMock.Setup(s => s.GetBulkCheckProgress(It.IsAny<string>()))
                   .ReturnsAsync(response);

            // Act

            var result = await _sut.Batch_Loader();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;
            viewResult.ActionName.Should().BeEquivalentTo("Batch_check_success");
        }

        [Test]
        public async Task Given_Batch_check_success_When_LoadingPage_Should_return_Batch_check_success()
        {
            // Act
            var result = await _sut.Batch_check_success();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().BeEquivalentTo("BatchOutcome/Success");
        }

        [Test]
        public async Task Given_Batch_check_download_When_LoadingPage_Should_return_csvFile()
        {
            //arrange
            var response =
              new CheckEligibilityBulkResponse
              {
                  Data = new List<CheckEligibilityItemFsm>() {
                  new CheckEligibilityItemFsm() {Status = CheckEligibilityStatus.eligible.ToString() } }
              };

            _checkServiceMock.Setup(s => s.GetBulkCheckResults(It.IsAny<string>()))
                   .ReturnsAsync(response);
            // Act
            var result = await _sut.Batch_check_download();

            // Assert
            result.Should().BeOfType<FileStreamResult>();
            var viewResult = result as FileStreamResult;
            viewResult.ContentType.Should().BeEquivalentTo("text/csv");
        }

    }
}

