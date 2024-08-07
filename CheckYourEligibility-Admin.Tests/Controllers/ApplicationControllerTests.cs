using CheckYourEligibility_FrontEnd.Controllers;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.Extensions.Logging;
using Moq;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using AutoFixture;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility.TestBase;

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
