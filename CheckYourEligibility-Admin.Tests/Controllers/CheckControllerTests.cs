using AutoFixture;
using AutoFixture.AutoMoq;
using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.TestBase;
using CheckYourEligibility_FrontEnd.Controllers;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Security.Claims;
using School = CheckYourEligibility_FrontEnd.Models.School;

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

            var request = _fixture.Create<ParentGuardian>();
            request.NationalInsuranceNumber = nino;
            request.NationalAsylumSeekerServiceNumber = nass;
            request.IsNassSelected = isNassSelected;
            request.Day = 1;
            request.Month = 1;
            request.Year = 1990;

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
            var request = _fixture.Create<ParentGuardian>();
            request.NationalInsuranceNumber = nino;
            request.NationalAsylumSeekerServiceNumber = nass;
            request.IsNassSelected = isNassSelected;
            request.Day = 1;
            request.Month = 1;
            request.Year = 1990;

            var response = _fixture.Create<CheckEligibilityResponse>();
            _checkServiceMock.Setup(x => x.PostCheck(It.IsAny<CheckEligibilityRequest>())).ReturnsAsync(response);

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

            _checkServiceMock.Invocations.Should().HaveCount(1); // PostCheck should have been called on the mocked service once

            if (isNassSelected)
            {
                _sut.HttpContext.Session.GetString("ParentNASS").Should().Be(request.NationalAsylumSeekerServiceNumber);
            }
            else
            {
                _sut.HttpContext.Session.GetString("ParentNINO").Should().Be(request.NationalInsuranceNumber?.ToUpper());
            }
        }

        [Test]
        public void Given_Loader_PageLoads()
        {
            // Act
            var result = _sut.Loader();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Loader");
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public void Enter_Child_Details_TempData_IsChildAddOrRemove_False_Returns_View_With_Empty_ChildList()
        {
            _tempData["IsChildAddOrRemove"] = false;

            var result = _sut.Enter_Child_Details();

            var viewResult = result as ViewResult;
            var model = viewResult.Model as Children;
            model.ChildList.Should().NotBeNullOrEmpty();
            model.ChildList[0].FirstName.Should().BeNull();
            model.ChildList[0].LastName.Should().BeNull();
        }

        [Test]
        public void Enter_Child_Details_TempData_IsChildAddOrRemove_True_Returns_View_With_ChildList_From_TempData()
        {
            _tempData["IsChildAddOrRemove"] = true;

            var children = _fixture.Create<List<Child>>();
            var childListJson = JsonConvert.SerializeObject(children);

            _tempData["ChildList"] = childListJson;

            var result = _sut.Enter_Child_Details();

            var viewResult = result as ViewResult;
            var model = viewResult.Model as Children;
            model.ChildList.Should().NotBeNullOrEmpty();
            model.ChildList[0].FirstName.Should().Be(children[0].FirstName);
            model.ChildList[0].LastName.Should().Be(children[0].LastName);
        }

        [Test]
        public void Given_Enter_Child_Details_When_TempData_IsRedirect_IsTrue_Returns_View_With_Request()
        {
            // Arrange
            _tempData["FsmApplication"] = _fixture.Create<FsmApplication>();
            _tempData["IsRedirect"] = true;
            var request = _fixture.Create<Children>();

            // Act
            var result = _sut.Enter_Child_Details(request);

            // Assert
            var viewResult = result as ViewResult;
            var model = viewResult.Model;

            viewResult.ViewName.Should().Be("Enter_Child_Details");
            model.Should().BeSameAs(request);
            _sut.ModelState.IsValid.Should().Be(true);
        }

        [Test]
        public void Given_Enter_Child_Details_When_ModelState_IsNotValid_Returns_SameView_With_Request()
        {
            // Arrange
            _sut.ModelState.AddModelError("Error Key", "Error Message");
            var request = _fixture.Create<Children>();

            // Act
            var result = _sut.Enter_Child_Details(request);

            // Assert
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Children;

            viewResult.ViewName.Should().Be("Enter_Child_Details");
            model.Should().BeSameAs(request);
            _sut.ModelState.IsValid.Should().Be(false);
        }

        [Test]
        public void Given_Enter_Child_Details_When_ModelState_IsValid_Returns_Check_Answers_View_With_FsmApplication()
        {
            // Arrange
            var request = _fixture.Create<Children>();

            // Act
            var result = _sut.Enter_Child_Details(request);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Check_Answers");
            var model = viewResult.Model as FsmApplication;
            model.Children.Should().BeSameAs(request);
            _sut.ModelState.IsValid.Should().Be(true);
        }

        [Test]
        public void Given_Add_Child_When_ChildList_ContainsLessThan99Children_ItAddsNewChild_And_Redirects_To_Enter_Child_Details_Page()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            var children = _fixture.CreateMany<Child>(5).ToList();
            request.ChildList.Clear();
            request.ChildList = children;

            // Act
            var result = _sut.Add_Child(request);

            // Assert
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.ActionName.Should().Be("Enter_Child_Details");
            _sut.TempData["ChildList"].Should().NotBeNull();
            var convertedTempData = JsonConvert.DeserializeObject<List<Child>>(_sut.TempData["ChildList"] as string);
            convertedTempData.Count.Should().Be(6);
        }

        [Test]
        public void Given_Add_Child_When_ChildList_ContainsExactly99Children_ItDoesNotAddNewChild_And_Redirects_To_Enter_Child_Details_Page()
        {

            var request = _fixture.Create<Children>();
            var children = _fixture.CreateMany<Child>(99).ToList();
            request.ChildList.Clear();
            request.ChildList = children;

            var result = _sut.Add_Child(request);

            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.ActionName.Should().Be("Enter_Child_Details");
            _sut.TempData["ChildList"].Should().BeNull();
        }

        [Test]
        public void Given_Remove_Child_When_ValidIndexIsProvided_The_ChildIsRemovedFrom_ChildList_AtGivenIndex()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            var childListCount = request.ChildList.Count();

            // Act
            var result = _sut.Remove_Child(request, 1);

            // Assert
            _sut.TempData["ChildList"].Should().NotBeNull();
            var convertedTempData = JsonConvert.DeserializeObject<List<Child>>(_sut.TempData["ChildList"] as string);
            convertedTempData.Count.Should().Be(childListCount - 1); // has 1 less
        }

        [Test]
        public void Given_Remove_Child_When_InvalidIndexIsProvided_NoChild_IsRemovedFrom_ChildList_AtGivenIndex_And_ArgumentOutOfRangeException_IsThrown()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            var childListCount = request.ChildList.Count();
            var nonExistantIndex = childListCount + 1;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Remove_Child(request, nonExistantIndex));
            _sut.TempData["ChildList"].Should().BeNull();
        }

        [Test]
        public void Given_Check_Answers_PageLoads()
        {
            // Act
            var result = _sut.Check_Answers();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Check_Answers");
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public void Given_Check_Answers_With_Valid_FsmApplication_RedirectsTo_ApplicationRegistered_Page()
        {
            // Arrange
            var serviceMockRequest = _fixture.Create<ApplicationRequest>();
            var serviceMockResponse = _fixture.Create<Task<ApplicationSaveItemResponse>>();
            var userCreateResponse = _fixture.Create<UserSaveItemResponse>();
            var request = _fixture.Create<FsmApplication>();
            _parentServiceMock.Setup(x => x.CreateUser(It.IsAny<UserCreateRequest>()))
                .ReturnsAsync(userCreateResponse);
            _parentServiceMock.Setup(x => x.PostApplication(It.IsAny<ApplicationRequest>()))
                .Returns(serviceMockResponse);

            // Act
            var result = _sut.Check_Answers(request);
            var tempData = JsonConvert.DeserializeObject<ApplicationConfirmationEntitledViewModel>(_sut.TempData["confirmationApplication"] as string);

            // Assert
            var redirectToActionResult = result.Result as RedirectToActionResult;
            redirectToActionResult.ActionName.Should().Be("ApplicationsRegistered");
            tempData.Children[0].ChildName.Should().Be($"{serviceMockResponse.Result.Data.ChildFirstName} {serviceMockResponse.Result.Data.ChildLastName}");

        }

        [Test]
        public void Given_Check_Answers_With_Invalid_FsmApplication_ThrowsException()
        {
            // Arrange
            var request = new FsmApplication(); // Invalid request with missing required fields

            _parentServiceMock.Setup(x => x.PostApplication(It.IsAny<ApplicationRequest>()))
                .Throws(new NullReferenceException("Invalid request"));

            // Act & Assert
            Assert.ThrowsAsync<NullReferenceException>(() => _sut.Check_Answers(request));
        }

        [Test]
        public void Given_ApplicationsRegistered_It_ReturnsView_With_ApplicationConfirmationEntitledViewModel()
        {
            // Arrange
            var fixture = _fixture.Create<ApplicationConfirmationEntitledViewModel>();
            _sut.TempData["confirmationApplication"] = JsonConvert.SerializeObject(fixture);

            // Act
            var result = _sut.ApplicationsRegistered();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("ApplicationsRegistered");
            viewResult.Model.Should().NotBeNull();
            var model = viewResult.Model as ApplicationConfirmationEntitledViewModel;
            model.Children[0].ChildName.Should().Be(fixture.Children[0].ChildName);
            model.Children[0].ParentName.Should().Be(fixture.Children[0].ParentName);
            model.Children[1].ChildName.Should().Be(fixture.Children[1].ChildName);
        }

        [Test]
        public void Given_ChangeChildDetails_With_Valid_TempData_Returns_View()
        {
            // Arrange
            var fsmApplication = _fixture.Create<FsmApplication>();

            _sut.TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);

            // Act
            int child = 0;
            var result = _sut.ChangeChildDetails(child);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Enter_Child_Details");
            var model = viewResult.Model as Children;
            model.Should().BeEquivalentTo(fsmApplication.Children);
        }

        [TestCase(CheckEligibilityStatus.eligible, "Outcome/Eligible")]
        [TestCase(CheckEligibilityStatus.notEligible, "Outcome/Not_Eligible")]
        [TestCase(CheckEligibilityStatus.parentNotFound, "Outcome/Not_Found")]
        [TestCase(CheckEligibilityStatus.DwpError, "Outcome/Not_Found_Pending")]
        public async Task Given_Poll_Status_With_Valid_Status_Returns_Correct_View(CheckEligibilityStatus status, string expectedView)
        {
            // Arrange

            // Build status value fixture first since AutoFixture With() does not allow assignment on nested properties like x.Data.Status
            var statusValue = _fixture.Build<StatusValue>()
                .With(x => x.Status, status.ToString())
                .Create();

            var checkEligibilityResponse = _fixture.Build<CheckEligibilityResponse>()
                .With(x => x.Data, statusValue)
                .Create();

            var responseJson = JsonConvert.SerializeObject(checkEligibilityResponse);
            _tempData["Response"] = responseJson;

            var checkEligibilityStatusResponse = _fixture.Build<CheckEligibilityStatusResponse>()
                .With(x => x.Data, checkEligibilityResponse.Data)
                .Create();

            _checkServiceMock.Setup(x => x.GetStatus(It.IsAny<CheckEligibilityResponse>()))
                .ReturnsAsync(checkEligibilityStatusResponse);

            // Mock the _Claims object with all necessary claims
            _sut.ControllerContext.HttpContext = new DefaultHttpContext();
            _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
    {
        new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "12345"),
        new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@example.com"),
        new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "John"),
        new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "Doe"),
        new Claim("OrganisationCategoryName", CheckYourEligibility_FrontEnd.Models.Constants.CategoryTypeLA)
    }));

            // Act
            var result = await _sut.Poll_Status();

            // Assert
            if (result is PartialViewResult partialViewResult)
            {
                partialViewResult.ViewName.Should().Be(expectedView);
            }
            else if (result is RedirectToActionResult redirectResult)
            {
                redirectResult.ActionName.Should().Be("Application_Sent"); // Adjust this if you expect a different action
            }
            else
            {
                Assert.Fail("Unexpected result type");
            }
        }

        [Test]
        public async Task Given_Poll_Status_When_Response_Is_Null_Returns_Error_Status()
        {
            // Arrange
            _tempData["Response"] = null;

            // Act
            var result = await _sut.Poll_Status();

            // Assert
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            jsonResult.Value.Should().BeEquivalentTo(new { status = "error", message = "No response data found" });
        }

        [Test]
        public async Task Given_Poll_Status_When_Status_Is_Processing_Returns_JsonResult_Processing()
        {
            // Arrange
            var response = new CheckEligibilityResponse
            {
                Data = new StatusValue { Status = "processing" }
            };
            _tempData["Response"] = JsonConvert.SerializeObject(response);

            _checkServiceMock.Setup(x => x.GetStatus(It.IsAny<CheckEligibilityResponse>()))
                .ReturnsAsync(new CheckEligibilityStatusResponse { Data = response.Data });

            // Act
            var result = await _sut.Poll_Status();

            // Assert
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            jsonResult.Value.Should().BeEquivalentTo(new { status = "processing" });
        }




    }
}
