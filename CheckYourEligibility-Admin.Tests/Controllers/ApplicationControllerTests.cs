using AutoFixture;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.TestBase;
using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_FrontEnd.Controllers;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility_Admin.Tests.Controllers
{
    [TestFixture]
    public class ApplicationControllerTests : TestBase
    {
        //mocks
        private ILogger<ApplicationController> _loggerMock;
        private Mock<IEcsServiceAdmin> _adminServiceMock;

        // system under test
        private ApplicationController _sut;

        [SetUp]
        public void SetUp()
        {
            _adminServiceMock = new Mock<IEcsServiceAdmin>();
            _loggerMock = Mock.Of<ILogger<ApplicationController>>();
            _sut = new ApplicationController(_loggerMock, _adminServiceMock.Object);

            base.SetUp();
            _sut.ControllerContext.HttpContext = _httpContext.Object;
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        #region search

        [Test]
        public async Task Given_Application_Search_Should_Load_ApplicationSearchPage()
        {
            // Arrange 
            _sut.TempData = _tempData;

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
            _sut.TempData = _tempData;
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

        [Test]
        public async Task Given_ApplicationDetail_Results_Page_Returns_Valid_Data()
        {
            //arrange
            var response = _fixture.Create<ApplicationItemResponse>();
            var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
            response.Data.School.Id = Convert.ToInt32(claims.Organisation.Urn);

            _adminServiceMock.Setup(s => s.GetApplication(It.IsAny<string>()))
                   .ReturnsAsync(response);

            var request = new ApplicationSearch();

            //act
            var result = await _sut.ApplicationDetail(response.Data.Id);

            //assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeAssignableTo<ApplicationItemResponse>();

            var model = viewResult.Model as ApplicationItemResponse;
            model.Should().NotBeNull();
            model.Should().BeEquivalentTo(response);

        }

        [Test]
        public async Task Given_ApplicationDetail_Results_Returns_NotFound()
        {
            //arrange
            var response = _fixture.Create<ApplicationItemResponse>();
            var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
            response.Data.School.Id = Convert.ToInt32(claims.Organisation.Urn);

            _adminServiceMock.Setup(s => s.GetApplication(It.IsAny<string>()))
                   .ReturnsAsync(default(ApplicationItemResponse));

            var request = new ApplicationSearch();

            //act
            var result = await _sut.ApplicationDetail(response.Data.Id);

            //assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Given_ApplicationDetail_Results_Returns_UnauthorizedResult()
        {
            //arrange
            var response = _fixture.Create<ApplicationItemResponse>();
            response.Data.School.Id = -99;

            _adminServiceMock.Setup(s => s.GetApplication(It.IsAny<string>()))
                   .ReturnsAsync(response);

            var request = new ApplicationSearch();

            //act
            var result = await _sut.ApplicationDetail(response.Data.Id);

            //assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        #endregion

        #region appeals


        [Test]
        public async Task Given_Process_Appeals_Results_Page_Returns_Valid_Data()
        {
            //arrange
            var response = _fixture.Create<ApplicationSearchResponse>();

            _adminServiceMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
                   .ReturnsAsync(response);

            var request = new ApplicationSearch();

            //act
            var result = await _sut.Process_Appeals();

            //assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeAssignableTo<ApplicationSearchResponse>();

            var model = viewResult.Model as ApplicationSearchResponse;
            model.Should().NotBeNull();
            model.Should().BeEquivalentTo(response);

        }

        [Test]
        public async Task Given_Process_Appeals_Returns_No_Records_null()
        {
            //Arrange
            _sut.TempData = _tempData;
            _adminServiceMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
                .ReturnsAsync(default(ApplicationSearchResponse));

            var request = new ApplicationSearch();

            //act
            var result = await _sut.Process_Appeals();

            //assert 
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var resultData = viewResult.Model as ApplicationSearchResponse;
            resultData.Data.Should().BeEmpty();
        }

        [Test]
        public async Task Given_Process_Appeals_Finalise_Returns_ViewResult()
        {
            //Arrange
            //act
            var result = _sut.Finalise();

            //assert 
            result.Should().BeOfType<ViewResult>();
        }

        [Test]
        public async Task Given_Process_Appeals_EvidenceGuidance_Returns_ViewResult()
        {
            //Arrange
            //act
            var result = _sut.Finalise();

            //assert 
            result.Should().BeOfType<ViewResult>();
        }

        [Test]
        public async Task Given_ApplicationDetailAppeal_Results_Page_Returns_Valid_Data()
        {
            //arrange
            var response = _fixture.Create<ApplicationItemResponse>();
            var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
            response.Data.School.Id = Convert.ToInt32(claims.Organisation.Urn);

            _adminServiceMock.Setup(s => s.GetApplication(It.IsAny<string>()))
                   .ReturnsAsync(response);

            var request = new ApplicationSearch();

            //act
            var result = await _sut.ApplicationDetailAppeal(response.Data.Id);

            //assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeAssignableTo<ApplicationItemResponse>();

            var model = viewResult.Model as ApplicationItemResponse;
            model.Should().NotBeNull();
            model.Should().BeEquivalentTo(response);

        }

        [Test]
        public async Task Given_ApplicationDetailAppeal_Results_Returns_NotFound()
        {
            //arrange
            var response = _fixture.Create<ApplicationItemResponse>();
            var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
            response.Data.School.Id = Convert.ToInt32(claims.Organisation.Urn);

            _adminServiceMock.Setup(s => s.GetApplication(It.IsAny<string>()))
                   .ReturnsAsync(default(ApplicationItemResponse));

            var request = new ApplicationSearch();

            //act
            var result = await _sut.ApplicationDetailAppeal(response.Data.Id);

            //assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Given_ApplicationDetailAppeal_Results_Returns_UnauthorizedResult()
        {
            //arrange
            var response = _fixture.Create<ApplicationItemResponse>();
            response.Data.School.Id = -99;

            _adminServiceMock.Setup(s => s.GetApplication(It.IsAny<string>()))
                   .ReturnsAsync(response);

            var request = new ApplicationSearch();

            //act
            var result = await _sut.ApplicationDetailAppeal(response.Data.Id);

            //assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Test]
        public async Task Given_ApplicationDetailAppealConfirmation_Returns_ViewResult()
        {
            //Arrange
            _sut.TempData = _tempData;
            var id = _fixture.Create<string>();
            //act
            var result = await _sut.ApplicationDetailAppealConfirmation(id);

            //assert 
            result.Should().BeOfType<ViewResult>();
            _sut.TempData["AppAppealID"].Should().Be(id);
        }

        [Test]
        public async Task Given_ApplicationDetailAppealSend_Returns_ViewResult()
        {
            //Arrange
            var id = _fixture.Create<string>();
            //act
            var result = await _sut.ApplicationDetailAppealSend(id);

            //assert 
            result.Should().BeOfType<RedirectToActionResult>();
            var redirect = result as RedirectToActionResult;
            redirect.ActionName.Should().BeEquivalentTo("Process_Appeals");
        }

        #endregion
    }
}
