using AutoFixture;
using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.TestBase;
using CheckYourEligibility_FrontEnd.Controllers;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using CheckYourEligibility_FrontEnd.ViewModels;
using CsvHelper.Configuration.Attributes;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Security.Claims;
using Child = CheckYourEligibility_FrontEnd.Models.Child;
using School = CheckYourEligibility_FrontEnd.Models.School;

namespace CheckYourEligibility_Admin.Tests.Controllers
{
    [TestFixture]
    public class CheckControllerTests : TestBase
    {
        // Mocks for use cases
        private ILogger<CheckController> _loggerMock;
        private Mock<IAdminLoadParentDetailsUseCase> _adminLoadParentDetailsUseCaseMock;
        private Mock<IAdminProcessParentDetailsUseCase> _adminProcessParentDetailsUseCaseMock;
        private Mock<IAdminEnterChildDetailsUseCase> _adminEnterChildDetailsUseCaseMock;
        private Mock<IAdminProcessChildDetailsUseCase> _adminProcessChildDetailsUseCaseMock;
        private Mock<IAdminAddChildUseCase> _adminAddChildUseCaseMock;
        private Mock<IAdminLoaderUseCase> _adminLoaderUseCaseMock;
        private Mock<IAdminRemoveChildUseCase> _adminRemoveChildUseCaseMock;
        private Mock<IAdminChangeChildDetailsUseCase> _adminChangeChildDetailsUseCaseMock;
        private Mock<IAdminRegistrationResponseUseCase> _adminRegistrationResponseUseCaseMock;
        private Mock<IAdminApplicationsRegisteredUseCase> _adminApplicationsRegisteredUseCaseMock;
        private Mock<IAdminCreateUserUseCase> _adminCreateUserUseCaseMock;
        private Mock<IAdminSubmitApplicationUseCase> _adminSubmitApplicationUseCaseMock;
        private Mock<IAdminValidateParentDetailsUseCase> _adminValidateParentDetailsUseCaseMock;
        private Mock<IAdminInitializeCheckAnswersUseCase> _adminInitializeCheckAnswersUseCaseMock;

        // Legacy service mocks - keep temporarily during transition
        private Mock<IEcsServiceParent> _parentServiceMock;
        private Mock<IEcsCheckService> _checkServiceMock;

        // System under test
        private CheckController _sut;

        [SetUp]
        public void SetUp()
        {
            // Initialize legacy service mocks
            _parentServiceMock = new Mock<IEcsServiceParent>();
            _checkServiceMock = new Mock<IEcsCheckService>();
            _loggerMock = Mock.Of<ILogger<CheckController>>();

            // Initialize use case mocks
            _adminLoadParentDetailsUseCaseMock = new Mock<IAdminLoadParentDetailsUseCase>();
            _adminProcessParentDetailsUseCaseMock = new Mock<IAdminProcessParentDetailsUseCase>();
            _adminEnterChildDetailsUseCaseMock = new Mock<IAdminEnterChildDetailsUseCase>();
            _adminProcessChildDetailsUseCaseMock = new Mock<IAdminProcessChildDetailsUseCase>();
            _adminAddChildUseCaseMock = new Mock<IAdminAddChildUseCase>();
            _adminLoaderUseCaseMock = new Mock<IAdminLoaderUseCase>();
            _adminRemoveChildUseCaseMock = new Mock<IAdminRemoveChildUseCase>();
            _adminChangeChildDetailsUseCaseMock = new Mock<IAdminChangeChildDetailsUseCase>();
            _adminRegistrationResponseUseCaseMock = new Mock<IAdminRegistrationResponseUseCase>();
            _adminApplicationsRegisteredUseCaseMock = new Mock<IAdminApplicationsRegisteredUseCase>();
            _adminCreateUserUseCaseMock = new Mock<IAdminCreateUserUseCase>();
            _adminSubmitApplicationUseCaseMock = new Mock<IAdminSubmitApplicationUseCase>();
            _adminValidateParentDetailsUseCaseMock = new Mock<IAdminValidateParentDetailsUseCase>();
            _adminInitializeCheckAnswersUseCaseMock = new Mock<IAdminInitializeCheckAnswersUseCase>();

            // Initialize controller with all dependencies
            _sut = new CheckController(
                _loggerMock,
                _parentServiceMock.Object,
                _checkServiceMock.Object,
                _configMock.Object,
                _adminLoadParentDetailsUseCaseMock.Object,
                _adminProcessParentDetailsUseCaseMock.Object,
                _adminEnterChildDetailsUseCaseMock.Object,
                _adminProcessChildDetailsUseCaseMock.Object,
                _adminAddChildUseCaseMock.Object,
                _adminLoaderUseCaseMock.Object,
                _adminRemoveChildUseCaseMock.Object,
                _adminChangeChildDetailsUseCaseMock.Object,
                _adminRegistrationResponseUseCaseMock.Object,
                _adminApplicationsRegisteredUseCaseMock.Object,
                _adminCreateUserUseCaseMock.Object,
                _adminSubmitApplicationUseCaseMock.Object,
                _adminValidateParentDetailsUseCaseMock.Object,
                _adminInitializeCheckAnswersUseCaseMock.Object
            );

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
        public async Task Enter_Details_Get_When_NoResponseInTempData_Should_ReturnView()
        {
            // Arrange
            var expectedParent = _fixture.Create<ParentGuardian>();
            var expectedErrors = new Dictionary<string, List<string>>();

            _adminLoadParentDetailsUseCaseMock
                .Setup(x => x.Execute(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((expectedParent, expectedErrors));

            // Act
            var result = await _sut.Enter_Details();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().Be(expectedParent);
        }

        [Test]
        [TestCase(0, "AB123456C", null)] // NinSelected = 0
        [TestCase(1, null, "2407001")]   // AsrnSelected = 1
        public async Task Enter_Details_Post_When_ValidationFails_Should_RedirectBack(
    int ninAsrSelectValue,
    string? nino,
    string? nass)
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.NationalInsuranceNumber = nino;
            request.NationalAsylumSeekerServiceNumber = nass;
            request.NinAsrSelection = (ParentGuardian.NinAsrSelect)ninAsrSelectValue;
            request.Day = "1";
            request.Month = "1";
            request.Year = "1990";

            var validationResult = new ValidationResult
            {
                IsValid = false,
                Errors = new Dictionary<string, List<string>>
        {
            { "Error Key", new List<string> { "Error Message" } }
        }
            };

            _adminValidateParentDetailsUseCaseMock
                .Setup(x => x.Execute(request, It.IsAny<ModelStateDictionary>()))
                .Returns(validationResult);

            // Act
            var result = await _sut.Enter_Details(request);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Enter_Details");

            // Verify TempData contains expected values
            _sut.TempData.Should().ContainKey("ParentDetails");
            _sut.TempData.Should().ContainKey("Errors");

            // Verify the mock was called with correct parameters
            _adminValidateParentDetailsUseCaseMock.Verify(
                x => x.Execute(request, It.IsAny<ModelStateDictionary>()),
                Times.Once);
        }

        [Test]
        [TestCase(ParentGuardian.NinAsrSelect.NinSelected, "AB123456C", null)]
        [TestCase(ParentGuardian.NinAsrSelect.AsrnSelected, null, "2407001")]
        public async Task Enter_Details_Post_When_Valid_Should_ProcessAndRedirectToLoader(
    ParentGuardian.NinAsrSelect ninasSelection,
    string? nino,
    string? nass)
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.NationalInsuranceNumber = nino;
            request.NationalAsylumSeekerServiceNumber = nass;
            request.NinAsrSelection = ninasSelection;
            request.Day = "01";
            request.Month = "01";
            request.Year = "1990";

            var validationResult = new ValidationResult { IsValid = true };
            var checkEligibilityResponse = _fixture.Create<CheckEligibilityResponse>();

            _adminValidateParentDetailsUseCaseMock
                .Setup(x => x.Execute(request, It.IsAny<ModelStateDictionary>()))
                .Returns(validationResult);

            _adminProcessParentDetailsUseCaseMock
                .Setup(x => x.Execute(request, _sut.HttpContext.Session))
                .ReturnsAsync(checkEligibilityResponse);

            // Act
            var result = await _sut.Enter_Details(request);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Loader");
            _sut.TempData["Response"].Should().NotBeNull();

            _adminValidateParentDetailsUseCaseMock.Verify(
                x => x.Execute(request, It.IsAny<ModelStateDictionary>()),
                Times.Once);

            _adminProcessParentDetailsUseCaseMock.Verify(
                x => x.Execute(request, _sut.HttpContext.Session),
                Times.Once);
        }


        [Test]
        public void Enter_Child_Details_Get_Should_Handle_Initial_Load()
        {
            // Arrange
            var expectedResult = new AdminEnterChildDetailsResult
            {
                Children = new Children { ChildList = new List<Child> { new Child() } },
                IsRedirect = false,
                ModelState = new ModelStateDictionary()
            };

            _adminEnterChildDetailsUseCaseMock
                .Setup(x => x.Execute(
                    It.IsAny<bool?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = _sut.Enter_Child_Details() as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result.Model.Should().BeEquivalentTo(expectedResult.Children);
        }

        [Test]
        public void Enter_Child_Details_Post_When_Valid_Should_Process_And_Return_CheckAnswers()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            var fsmApplication = _fixture.Create<FsmApplication>();

            _adminProcessChildDetailsUseCaseMock
                .Setup(x => x.Execute(request, _sut.HttpContext.Session))
                .ReturnsAsync(fsmApplication);

            // Act
            var result = _sut.Enter_Child_Details(request);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Check_Answers");
            viewResult.Model.Should().Be(fsmApplication);

            _adminProcessChildDetailsUseCaseMock.Verify(
                x => x.Execute(request, _sut.HttpContext.Session),
                Times.Once);
        }

        [Test]
        public async Task Add_Child_Should_Execute_UseCase_And_Redirect()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            var updatedChildren = _fixture.Create<Children>();

            _adminAddChildUseCaseMock
                .Setup(x => x.Execute(request))
                .Returns(updatedChildren);

            // Act
            var result = _sut.Add_Child(request);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Enter_Child_Details");
        }
        [Test]
        public async Task Remove_Child_Should_Execute_UseCase_And_Redirect()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            var expectedChildren = new Children
            {
                ChildList = new List<Child> { _fixture.Create<Child>() }
            };
            const int index = 1;

            _adminRemoveChildUseCaseMock
                .Setup(x => x.Execute(It.IsAny<Children>(), index))
                .ReturnsAsync(expectedChildren);

            // Act
            var result = await _sut.Remove_Child(request, index);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Enter_Child_Details");

            _adminRemoveChildUseCaseMock.Verify(
                x => x.Execute(It.IsAny<Children>(), index),
                Times.Once);

            _sut.TempData["IsChildAddOrRemove"].Should().Be(true);
            var serializedChildren = _sut.TempData["ChildList"] as string;
            serializedChildren.Should().NotBeNull();
            var deserializedChildren = JsonConvert.DeserializeObject<List<Child>>(serializedChildren);
            deserializedChildren.Should().BeEquivalentTo(expectedChildren.ChildList);
        }

        [Test]
        public async Task Remove_Child_When_InvalidIndex_Should_Throw_Exception()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            const int invalidIndex = 999;
            _adminRemoveChildUseCaseMock
                .Setup(x => x.Execute(request, invalidIndex))
                .ThrowsAsync(new ArgumentOutOfRangeException());

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await _sut.Remove_Child(request, invalidIndex));

            // Additional assertions if needed
            exception.Should().NotBeNull();
        }

        [Test]
        public void Check_Answers_Get_Should_Return_View()
        {
            // Act
            var result = _sut.Check_Answers();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Check_Answers");
        }

        [Test]
        public async Task Check_Answers_Post_Should_Submit_And_RedirectTo_AppealsRegistered()
        {
            // Arrange
            var request = _fixture.Create<FsmApplication>();
            var userId = "test-user-id";
            var lastResponse = new ApplicationSaveItemResponse
            {
                Data = new ApplicationResponse { Status = "NotEntitled" }
            };

            _adminCreateUserUseCaseMock
                .Setup(x => x.Execute(It.IsAny<IEnumerable<Claim>>()))
                .ReturnsAsync(userId);

            _adminSubmitApplicationUseCaseMock
                .Setup(x => x.Execute(request, userId, It.IsAny<string>()))
                .ReturnsAsync((new ApplicationConfirmationEntitledViewModel(), lastResponse));

            // Act
            var result = await _sut.Check_Answers(request);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("AppealsRegistered");
        }


        [Test]
        public async Task Check_Answers_Post_Should_Submit_And_RedirectTo_ApplicationsRegistered()
        {
            // Arrange
            var request = _fixture.Create<FsmApplication>();
            var userId = "test-user-id";
            var viewModel = _fixture.Create<ApplicationConfirmationEntitledViewModel>();
            var lastResponse = new ApplicationSaveItemResponse
            {
                Data = new ApplicationResponse { Status = "Entitled" }
            };

            _adminCreateUserUseCaseMock
                .Setup(x => x.Execute(It.IsAny<IEnumerable<Claim>>()))
                .ReturnsAsync(userId);

            _adminSubmitApplicationUseCaseMock
                .Setup(x => x.Execute(request, userId, It.IsAny<string>()))
                .ReturnsAsync((viewModel, lastResponse));

            // Act
            var result = await _sut.Check_Answers(request);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("ApplicationsRegistered");
        }

        [Test]
        public async Task Check_Answers_Post_With_Invalid_Application_Should_ThrowException()
        {
            // Arrange
            var request = new FsmApplication();
            var userId = "test-user-id";

            _adminCreateUserUseCaseMock
                .Setup(x => x.Execute(It.IsAny<IEnumerable<Claim>>()))
                .ReturnsAsync(userId);

            _adminSubmitApplicationUseCaseMock
                .Setup(x => x.Execute(request, userId, It.IsAny<string>()))
                .ThrowsAsync(new NullReferenceException("Invalid request"));

            // Act & Assert
            try
            {
                await _sut.Check_Answers(request);
                Assert.Fail("Expected NullReferenceException was not thrown");
            }
            catch (NullReferenceException ex)
            {
                ex.Message.Should().Be("Invalid request");
            }

            _adminCreateUserUseCaseMock.Verify(
                x => x.Execute(It.IsAny<IEnumerable<Claim>>()),
                Times.Once);

            _adminSubmitApplicationUseCaseMock.Verify(
                x => x.Execute(request, userId, It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public void ApplicationsRegistered_Should_Process_And_Return_View()
        {
            // Arrange
            var expectedViewModel = _fixture.Create<ApplicationConfirmationEntitledViewModel>();
            var expectedResult = AdminApplicationsRegisteredResult.Success(expectedViewModel);
            _adminApplicationsRegisteredUseCaseMock
                .Setup(x => x.Execute(It.IsAny<string>()))
                .ReturnsAsync(expectedResult);
            _sut.TempData["confirmationApplication"] = JsonConvert.SerializeObject(expectedViewModel);

            // Act
            var result = _sut.ApplicationsRegistered();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("ApplicationsRegistered");
            viewResult.Model.Should().BeEquivalentTo(expectedViewModel);
        }

        [Test]
        public void ChangeChildDetails_Should_Process_And_Return_View()
        {
            // Arrange
            var childIndex = 0;
            var fsmApplication = _fixture.Create<FsmApplication>();
            var expectedChildren = fsmApplication.Children; // Use the same Children instance

            _sut.TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);

            _adminChangeChildDetailsUseCaseMock
                .Setup(x => x.Execute(It.IsAny<string>()))
                .Returns(expectedChildren);

            // Act
            var result = _sut.ChangeChildDetails(childIndex);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Enter_Child_Details");

            
            var resultModel = viewResult.Model as Children;
            resultModel.Should().NotBeNull();
            resultModel.ChildList.Should().NotBeNull();
            resultModel.ChildList.Count.Should().Be(expectedChildren.ChildList.Count);

            _sut.TempData["IsRedirect"].Should().Be(true);
            _sut.TempData["childIndex"].Should().Be(childIndex);

            _adminChangeChildDetailsUseCaseMock.Verify(
                x => x.Execute(It.IsAny<string>()),
                Times.Once);
        }

        [TestCase(CheckEligibilityStatus.eligible, "Outcome/Eligible")]
        [TestCase(CheckEligibilityStatus.notEligible, "Outcome/Not_Eligible")]
        [TestCase(CheckEligibilityStatus.parentNotFound, "Outcome/Not_Found")]
        [TestCase(CheckEligibilityStatus.DwpError, "Outcome/Technical_Error")]
        public async Task Loader_With_Valid_Status_Should_Return_Correct_View(CheckEligibilityStatus status, string expectedView)
        {
            // Arrange
            var responseJson = _fixture.Create<string>();
            _tempData["Response"] = responseJson;

            // Setup the use case to return a result with the expected view, status and dummy data.
            _adminLoaderUseCaseMock
                .Setup(x => x.ExecuteAsync(responseJson, It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(AdminLoaderResult.Success(
                    expectedView,
                    status,
                    new StatusValue { Status = status.ToString() }
                ));

            // Act
            var result = await _sut.Loader();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be(expectedView);

            _sut.TempData["OutcomeStatus"].Should().Be(status);
            _adminLoaderUseCaseMock.Verify(
                x => x.ExecuteAsync(responseJson, It.IsAny<ClaimsPrincipal>()),
                Times.Once);
        }

        [Test]
        public async Task Loader_With_Null_Response_Should_Return_Error_View()
        {
            // Arrange
            _tempData["Response"] = null;

            // Act
            var result = await _sut.Loader();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Outcome/Technical_Error");
        }

        [Test]
        public async Task Loader_With_Processing_Status_Should_Return_Loader_View()
        {
            // Arrange
            var responseJson = _fixture.Create<string>();
            _tempData["Response"] = responseJson;

            _adminLoaderUseCaseMock
                .Setup(x => x.ExecuteAsync(responseJson, It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(AdminLoaderResult.Success(
                    "Loader",
                    CheckEligibilityStatus.queuedForProcessing,
                    new StatusValue { Status = CheckEligibilityStatus.queuedForProcessing.ToString() },
                    responseJson
                ));

            // Act
            var result = await _sut.Loader();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Loader");

            // When processing, the TempData should be re-populated with the response.
            _sut.TempData.Should().ContainKey("Response");
            _adminLoaderUseCaseMock.Verify(
                x => x.ExecuteAsync(responseJson, It.IsAny<ClaimsPrincipal>()),
                Times.Once);
        }
    }
}
