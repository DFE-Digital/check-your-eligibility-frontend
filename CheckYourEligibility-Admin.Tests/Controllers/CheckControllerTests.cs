using AutoFixture;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.TestBase;
using CheckYourEligibility_FrontEnd.Controllers;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility_Admin.Tests.Controllers
{
    [TestFixture]
    public class CheckControllerTests : TestBase
    {
        // mocks
        private ILogger<CheckController> _loggerMock;
        private Mock<IEcsServiceParent> _parentServiceMock;
        private Mock<IEcsCheckService> _checkServiceMock;

        // system under test
        private CheckController _sut;

        [SetUp]
        public void SetUp()
        {

            _parentServiceMock = new Mock<IEcsServiceParent>();
            _checkServiceMock = new Mock<IEcsCheckService>();
            _loggerMock = Mock.Of<ILogger<CheckController>>();

            _sut = new CheckController(_loggerMock, _parentServiceMock.Object, _checkServiceMock.Object, _configMock.Object);
           
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
        public void Given_Enter_Details_Should_Load_EnterDetailsPage()
        {
            // Act
            var result = _sut.Enter_Details();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeNull();
        }

        [Test]
        [TestCase(false, "AB123456C", null)]
        [TestCase(true, null, "2407001")]
        public void Given_Enter_Details_When_ModelState_Is_NotValid_Should_SetTempData_And_LoadEnter_DetailsPage(bool isNassSelected, string? nino, string? nass)
        {
            // Arrange
            _sut.ModelState.AddModelError("Error Key", "Error Message");

            var request = new ParentGuardian()
            {
                NationalInsuranceNumber = nino,
                FirstName = "Homer",
                LastName = "Simpson",
                Day = 31,
                Month = 12,
                Year = 2000,
                IsNassSelected = isNassSelected,
                NationalAsylumSeekerServiceNumber = nass,
                EmailAddress = "homer.simpson@madeupemailaddress.co.uk"
            };

            // Act
            var result = _sut.Enter_Details(request);

            // Assert
            var actionResult = result.Result as RedirectToActionResult;
            actionResult.ActionName.Should().Be("Enter_Details");

            var parentDetails = _sut.TempData["ParentDetails"] as string;
            var errors = _sut.TempData["Errors"] as string;

            parentDetails.Should().NotBeNull();
            errors.Should().NotBeNull();
        }

        [Test]
        [TestCase(false, "AB123456C", null)]
        [TestCase(true, null, "2407001")]
        public void Given_Enter_Details_When_ModelState_IsValid_Should_SetSessionData_CreateEligibilityRequest_And_LoadLoaderPage(bool isNassSelected, string? nino, string? nass)
        {
            // Arrange
            var response = _fixture.Create<CheckEligibilityResponse>();
            _checkServiceMock.Setup(x => x.PostCheck(It.IsAny<CheckEligibilityRequest>())).ReturnsAsync(response);


            var request = new ParentGuardian()
            {
                NationalInsuranceNumber = nino,
                FirstName = "Homer",
                LastName = "Simpson",
                Day = 31,
                Month = 12,
                Year = 2000,
                IsNassSelected = isNassSelected,
                NationalAsylumSeekerServiceNumber = nass,
                EmailAddress = "homer.simpson@madeupemailaddress.co.uk"
            };

            // Act
            var result = _sut.Enter_Details(request);

            // Assert
            var actionResult = result.Result as RedirectToActionResult;
            actionResult.Should().NotBeNull();
            actionResult.ActionName.Should().Be("Loader");


            _sut.HttpContext.Session.GetString("ParentFirstName").Should().Be(request.FirstName);
            _sut.HttpContext.Session.GetString("ParentLastName").Should().Be(request.LastName);
            _sut.HttpContext.Session.GetString("ParentDOB").Should().Be($"{request.Year.Value}-{request.Month.Value:D2}-{request.Day.Value:D2}");
            _sut.HttpContext.Session.GetString("ParentEmail").Should().Be(request.EmailAddress);

            if (isNassSelected)
            {
                _sut.HttpContext.Session.GetString("ParentNASS").Should().Be(request.NationalAsylumSeekerServiceNumber);
            }
            else
            {
                _sut.HttpContext.Session.GetString("ParentNINO").Should().Be(request.NationalInsuranceNumber?.ToUpper());
            }
        }
    }
}
