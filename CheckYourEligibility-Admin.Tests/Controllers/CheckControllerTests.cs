using AutoFixture;
using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_DfeSignIn.Models;
using CheckYourEligibility_FrontEnd.Controllers;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using CheckYourEligibility_FrontEnd.UseCases.Admin;
using CheckYourEligibility_FrontEnd.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Security.Claims;

namespace CheckYourEligibility_Admin.Tests.Controllers
{
    [TestFixture]
    public class CheckControllerTests : IDisposable
    {
        private Mock<ILogger<CheckController>> _loggerMock;
        private Mock<IEcsServiceParent> _parentServiceMock;
        private Mock<IEcsCheckService> _checkServiceMock;
        private Mock<IConfiguration> _configMock;
        private Mock<IAdminLoadParentDetailsUseCase> _adminLoadParentDetailsUseCaseMock;
        private Mock<IAdminProcessParentDetailsUseCase> _adminProcessParentDetailsUseCaseMock;
        private Mock<IAdminLoaderUseCase> _adminLoaderUseCaseMock;
        private Mock<IAdminEnterChildDetailsUseCase> _adminEnterChildDetailsUseCaseMock;
        private Mock<IAdminProcessChildDetailsUseCase> _adminProcessChildDetailsUseCaseMock;
        private Mock<IAdminAddChildUseCase> _adminAddChildUseCaseMock;
        private Mock<IAdminRemoveChildUseCase> _adminRemoveChildUseCaseMock;
        private Mock<IAdminChangeChildDetailsUseCase> _adminChangeChildDetailsUseCaseMock;
        private Mock<IAdminRegistrationResponseUseCase> _adminRegistrationResponseUseCaseMock;
        private Mock<HttpContext> _httpContextMock;
        private Mock<ISession> _sessionMock;
        private ITempDataDictionary _tempData;
        private Fixture _fixture;
        private CheckController _sut;
        private bool _disposed;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            SetupMocks();
            SetupController();
            SetupSessionAndTempData();
        }

        private void SetupMocks()
        {
            _loggerMock = new Mock<ILogger<CheckController>>();
            _parentServiceMock = new Mock<IEcsServiceParent>();
            _checkServiceMock = new Mock<IEcsCheckService>();
            _configMock = new Mock<IConfiguration>();
            _adminLoadParentDetailsUseCaseMock = new Mock<IAdminLoadParentDetailsUseCase>();
            _adminProcessParentDetailsUseCaseMock = new Mock<IAdminProcessParentDetailsUseCase>();
            _adminLoaderUseCaseMock = new Mock<IAdminLoaderUseCase>();
            _adminEnterChildDetailsUseCaseMock = new Mock<IAdminEnterChildDetailsUseCase>();
            _adminProcessChildDetailsUseCaseMock = new Mock<IAdminProcessChildDetailsUseCase>();
            _adminAddChildUseCaseMock = new Mock<IAdminAddChildUseCase>();
            _adminRemoveChildUseCaseMock = new Mock<IAdminRemoveChildUseCase>();
            _adminChangeChildDetailsUseCaseMock = new Mock<IAdminChangeChildDetailsUseCase>();
            _adminRegistrationResponseUseCaseMock = new Mock<IAdminRegistrationResponseUseCase>();
        }

        private void SetupController()
        {
            _sut = new CheckController(
                _loggerMock.Object,
                _parentServiceMock.Object,
                _checkServiceMock.Object,
                _configMock.Object,
                _adminLoadParentDetailsUseCaseMock.Object,
                _adminProcessParentDetailsUseCaseMock.Object,
                _adminLoaderUseCaseMock.Object,
                _adminEnterChildDetailsUseCaseMock.Object,
                _adminProcessChildDetailsUseCaseMock.Object,
                _adminAddChildUseCaseMock.Object,
                _adminRemoveChildUseCaseMock.Object,
                _adminChangeChildDetailsUseCaseMock.Object,
                _adminRegistrationResponseUseCaseMock.Object
            );
        }

        private void SetupSessionAndTempData()
        {
            _httpContextMock = new Mock<HttpContext>();
            _sessionMock = new Mock<ISession>();
            _httpContextMock.Setup(x => x.Session).Returns(_sessionMock.Object);

            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            var tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
            _tempData = tempDataDictionaryFactory.GetTempData(httpContext);
            _sut.TempData = _tempData;
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContextMock.Object
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _sut = null;
                _fixture = null;
                _tempData = null;
            }

            _disposed = true;
        }

        [Test]
        public async Task Enter_Details_Get_WhenSuccessful_ShouldReturnView()
        {
            // Arrange
            var parent = _fixture.Create<ParentGuardian>();
            var validationErrors = new Dictionary<string, List<string>>();

            _adminLoadParentDetailsUseCaseMock
                .Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((parent, validationErrors));

            // Act
            var result = await _sut.Enter_Details();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeEquivalentTo(parent);
        }

        [Test]
        public async Task Enter_Details_Post_WhenModelStateInvalid_ShouldRedirectWithErrors()
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            _sut.ModelState.AddModelError("test", "test error");

            // Act
            var result = await _sut.Enter_Details(request);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Enter_Details");
            _tempData.Should().ContainKey("ParentDetails");
            _tempData.Should().ContainKey("Errors");
        }

        [Test]
        public async Task Loader_WhenSuccessful_ShouldReturnView()
        {
            // Arrange
            var claims = new List<Claim>();
            _httpContextMock.Setup(x => x.User.Claims).Returns(claims);

            _adminLoaderUseCaseMock
                .Setup(x => x.Execute(It.IsAny<string>(), claims))
                .ReturnsAsync(("TestView", CheckEligibilityStatus.eligible));

            // Act
            var result = await _sut.Loader();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("TestView");
        }

        [Test]
        public async Task Enter_Child_Details_Get_WhenSuccessful_ShouldReturnView()
        {
            // Arrange
            var children = _fixture.Create<Children>();
            _adminEnterChildDetailsUseCaseMock
                .Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<bool?>()))
                .ReturnsAsync(children);

            // Act
            var result = await _sut.Enter_Child_Details();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeEquivalentTo(children);
        }

        [Test]
        public async Task Add_Child_WhenSuccessful_ShouldRedirectToEnterDetails()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            var updatedChildren = _fixture.Create<Children>();

            _adminAddChildUseCaseMock
                .Setup(x => x.Execute(request))
                .ReturnsAsync(updatedChildren);

            // Act
            var result = await _sut.Add_Child(request);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Enter_Child_Details");
            _tempData["IsChildAddOrRemove"].Should().Be(true);
            _tempData.Should().ContainKey("ChildList");
        }

        [Test]
        public async Task Check_Answers_Post_WhenSuccessful_ShouldRedirectToRegistration()
        {
            // Arrange
            var request = _fixture.Create<FsmApplication>();
            var response = _fixture.Create<ApplicationConfirmationEntitledViewModel>();

            _adminRegistrationResponseUseCaseMock
                .Setup(x => x.Execute(request))
                .ReturnsAsync(response);

            // Act
            var result = await _sut.Check_Answers(request);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("ApplicationsRegistered");
            _tempData.Should().ContainKey("confirmationApplication");
        }

        private DfeClaims SetupDfeClaims()
        {
            var claims = new List<Claim>
            {
                new Claim("email", "test@test.com"),
                new Claim("sub", "testId"),
                new Claim("organisation", JsonConvert.SerializeObject(new { urn = "12345" }))
            };

            _httpContextMock.Setup(x => x.User.Claims).Returns(claims);
            return DfeSignInExtensions.GetDfeClaims(claims);
        }

        [Test]
        public async Task AppealsRegistered_WhenCalled_ShouldReturnCorrectView()
        {
            // Arrange
            var viewModel = _fixture.Create<ApplicationConfirmationEntitledViewModel>();
            _tempData["confirmationApplication"] = JsonConvert.SerializeObject(viewModel);

            // Act
            var result = await _sut.AppealsRegistered();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("AppealsRegistered");
            viewResult.Model.Should().BeEquivalentTo(viewModel);
        }

        [Test]
        public async Task AppealsRegistered_WhenExceptionOccurs_ShouldReturnTechnicalError()
        {
            // Arrange
            _tempData["confirmationApplication"] = "invalid json";

            // Act
            var result = await _sut.AppealsRegistered();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("Outcome/Technical_Error");
        }

        [Test]
        public async Task Enter_Child_Details_Post_WhenSuccessful_ShouldProcessRequest()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            var fsmApplication = _fixture.Create<FsmApplication>();
            var validationErrors = new Dictionary<string, string[]>();

            _adminProcessChildDetailsUseCaseMock
                .Setup(x => x.Execute(request, _sessionMock.Object, validationErrors))
                .ReturnsAsync(fsmApplication);

            // Act
            var result = await _sut.Enter_Child_Details(request);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(fsmApplication);
            _tempData.Should().ContainKey("FsmApplication");
        }

        [Test]
        public async Task Enter_Child_Details_Post_WhenValidationFails_ShouldReturnWithErrors()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            var validationErrors = new Dictionary<string, string[]>();

            _adminProcessChildDetailsUseCaseMock
                .Setup(x => x.Execute(request, _sessionMock.Object, validationErrors))
                .ThrowsAsync(new AdminProcessChildDetailsException("Validation failed"));

            // Act
            var result = await _sut.Enter_Child_Details(request);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.ErrorCount.Should().Be(1);
            viewResult.Model.Should().Be(request);
        }

        [Test]
        public async Task Change_Child_Details_WhenSuccessful_ShouldReturnView()
        {
            // Arrange
            var children = _fixture.Create<Children>();

            _adminChangeChildDetailsUseCaseMock
                .Setup(x => x.Execute(It.IsAny<string>()))
                .ReturnsAsync(children);

            // Act
            var result = await _sut.ChangeChildDetails(1);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(children);
            _tempData["IsRedirect"].Should().Be(true);
            _tempData["childIndex"].Should().Be(1);
        }

        [Test]
        public async Task Change_Child_Details_WhenUnsuccessful_ShouldRedirectToEnterDetails()
        {
            // Arrange
            _adminChangeChildDetailsUseCaseMock
                .Setup(x => x.Execute(It.IsAny<string>()))
                .ReturnsAsync(new Children
                {
                    ChildList = new List<CheckYourEligibility_FrontEnd.Models.Child>()
                });

            // Act
            var result = await _sut.ChangeChildDetails(1);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Enter_Child_Details");
        }

        [Test]
        public async Task Remove_Child_WhenExceptionOccurs_ShouldReturnTechnicalError()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            _adminRemoveChildUseCaseMock
                .Setup(x => x.Execute(request, 0))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _sut.Remove_Child(request, 0);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("Outcome/Technical_Error");
        }

        [Test]
        public async Task Add_Child_WhenExceptionOccurs_ShouldReturnTechnicalError()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            _adminAddChildUseCaseMock
                .Setup(x => x.Execute(request))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _sut.Add_Child(request);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("Outcome/Technical_Error");
        }
    }
}