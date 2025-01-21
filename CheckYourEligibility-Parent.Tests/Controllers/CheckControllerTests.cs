using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Controllers;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Child = CheckYourEligibility_FrontEnd.Models.Child;
using ChildsSchool = CheckYourEligibility_FrontEnd.Models.School;
using School = CheckYourEligibility.Domain.Responses.Establishment;
using CheckYourEligibility_FrontEnd.UseCases;
using System.Security.Claims;
using System.Text;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;

namespace CheckYourEligibility_Parent.Tests.Controllers
{
    [TestFixture]
    public class CheckControllerTests
    {
        // mocks
        private ILogger<CheckController> _loggerMock;
        private Mock<IEcsServiceParent> _parentServiceMock;
        private Mock<IEcsCheckService> _checkServiceMock;
        private Mock<ISession> _sessionMock;
        private Mock<HttpContext> _httpContext;
        private Mock<IConfiguration> _configMock;
        private Mock<ISearchSchoolsUseCase> _searchSchoolsUseCaseMock;
        private Mock<ICreateUserUseCase> _createUserUseCaseMock;
        private Mock<ILoadParentDetailsUseCase> _loadParentDetailsUseCaseMock;
        private Mock<IProcessParentDetailsUseCase> _processParentDetailsUseCaseMock;
        private Mock<ILoadParentNassDetailsUseCase> _loadParentNassDetailsUseCaseMock;
        private Mock<ILoaderUseCase> _loadParentLoaderUseCaseMock;
        private Mock<IParentSignInUseCase> _parentSignInUseCaseMock;
        private Mock<IEnterChildDetailsUseCase> _enterChildDetailsUseCaseMock;
        private Mock<IProcessChildDetailsUseCase> _processChildDetailsUseCaseMock;


        // check eligibility responses
        private CheckEligibilityResponse _eligibilityResponse;
        private CheckEligibilityStatusResponse _eligibilityStatusResponse;
        private EstablishmentSearchResponse _schoolSearchResponse;
        private ApplicationSaveItemResponse _applicationSaveItemResponse;


        // test data entities
        private FsmApplication _fsmApplication;
        private ChildsSchool[] _schools;
        private Parent _parent;
        private Children _children;
        private Children _defaultChildren;

        // system under test
        private CheckController _sut;

        [SetUp]
        public void SetUp()
        {
            SetUpTestData();
            SetUpInitialMocks();
            SetUpTempData();
            SetUpSessionData();
            SetUpHTTPContext();
            SetUpServiceMocks();

            void SetUpTestData()
            {
                _defaultChildren = new Children
                {
                    ChildList = new List<Child> { new Child() }
                };

                _schools = new[]
                {
                    new ChildsSchool()
                    {
                        Name = "Springfield Elementary",
                        LA = "Springfield",
                        Postcode = "SP1 3LE",
                        URN = "100021"
                    },
                    new ChildsSchool()
                    {
                        Name = "Springfield Nursery",
                        LA = "Springfield",
                        Postcode = "SP1 3NU",
                        URN = "100011"
                    }
                };

                _parent = new Parent()
                {
                    FirstName = "Homer",
                    LastName = "Simpson",
                    Day = "1",
                    Month = "1",
                    Year = "1990",
                    NationalInsuranceNumber = "AB123456C",
                    NationalAsylumSeekerServiceNumber = null,
                    IsNinoSelected = true,
                    IsNassSelected = null,
                };

                _children = new Children()
                {
                    ChildList = [
                        new Child()
                        {
                            FirstName = "Bart",
                            LastName = "Simpson",
                            Day = "1",
                            Month = "1",
                            Year = "2015",
                            School = _schools[0]
                        },
                        new Child()
                        {
                            FirstName = "Lisa",
                            LastName = "Simpson",
                            Day = "1",
                            Month = "1",
                            Year = "2018",
                            School = _schools[0]
                        },
                        new Child()
                        {
                            FirstName = "Maggie",
                            LastName = "Simpson",
                            Day = "1",
                            Month = "1",
                            Year = "2020",
                            School = _schools[1]
                        }
                    ]
                };

                _fsmApplication = new FsmApplication()
                {
                    ParentFirstName = _parent.FirstName,
                    ParentLastName = _parent.LastName,
                    ParentDateOfBirth = "01/01/1990",
                    ParentNino = _parent.NationalInsuranceNumber,
                    ParentNass = _parent.NationalAsylumSeekerServiceNumber,
                    Children = _children
                };

                _eligibilityResponse = new CheckEligibilityResponse()
                {
                    Data = new StatusValue
                    {
                        Status = "queuedForProcessing"
                    },
                    Links = new CheckEligibilityResponseLinks
                    {
                        Get_EligibilityCheck = "",
                        Get_EligibilityCheckStatus = "",
                        Put_EligibilityCheckProcess = ""
                    }
                };

                _eligibilityStatusResponse = new CheckEligibilityStatusResponse()
                {
                    Data = new StatusValue
                    {
                        Status = "eligible"
                    }
                };

                _schoolSearchResponse = new EstablishmentSearchResponse()
                {
                    Data =
                    [
                        new School()
                        {
                            Id = 100,
                            County = "Test County",
                            Distance = 0.05d,
                            La = "Test LA",
                            Locality = "Test Locality",
                            Name = "Test School",
                            Postcode = "TE55 5ST",
                            Street = "Test Street",
                            Town = "Test Town"
                        }
                    ]
                };

                _applicationSaveItemResponse = new ApplicationSaveItemResponse()
                {
                    Data = new ApplicationResponse()
                    {
                        ParentFirstName = _fsmApplication.ParentFirstName,
                        ParentLastName = _fsmApplication.ParentLastName,
                        ParentDateOfBirth = _fsmApplication.ParentDateOfBirth,
                        ParentNationalInsuranceNumber = _fsmApplication.ParentNino,
                        ChildFirstName = _fsmApplication.Children.ChildList[0].FirstName,
                        ChildLastName = _fsmApplication.Children.ChildList[0].LastName,
                        ChildDateOfBirth = new DateOnly(int.Parse(_fsmApplication.Children.ChildList[0].Year),
                        int.Parse(_fsmApplication.Children.ChildList[0].Month),
                        int.Parse(_fsmApplication.Children.ChildList[0].Day)).ToString("dd/MM/yyyy"),
                        ParentNationalAsylumSeekerServiceNumber = _fsmApplication.ParentNass,
                        Id = "",
                        Establishment = new ApplicationResponse.ApplicationEstablishment { Id = 10002, LocalAuthority = new ApplicationResponse.ApplicationEstablishment.EstablishmentLocalAuthority { Id = 123 } },
                        Reference = "",
                    },
                    Links = new ApplicationResponseLinks()
                    {
                        get_Application = ""
                    }
                };
            }

            void SetUpTempData()
            {
                ITempDataProvider tempDataProvider = Mock.Of<ITempDataProvider>();
                TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
                ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());
                _sut.TempData = tempData;
            }

            void SetUpInitialMocks()
            {
                _configMock = new Mock<IConfiguration>();
                _parentServiceMock = new Mock<IEcsServiceParent>();
                _checkServiceMock = new Mock<IEcsCheckService>();
                _loggerMock = Mock.Of<ILogger<CheckController>>();
                _searchSchoolsUseCaseMock = new Mock<ISearchSchoolsUseCase>();
                _createUserUseCaseMock = new Mock<ICreateUserUseCase>();
                _loadParentDetailsUseCaseMock = new Mock<ILoadParentDetailsUseCase>();
                _processParentDetailsUseCaseMock = new Mock<IProcessParentDetailsUseCase>();
                _loadParentNassDetailsUseCaseMock = new Mock<ILoadParentNassDetailsUseCase>();
                _loadParentLoaderUseCaseMock = new Mock<ILoaderUseCase>();
                _parentSignInUseCaseMock = new Mock<IParentSignInUseCase>();
                _enterChildDetailsUseCaseMock = new Mock<IEnterChildDetailsUseCase>();
                _processChildDetailsUseCaseMock = new Mock<IProcessChildDetailsUseCase>();
                

                _sut = new CheckController(
                    _loggerMock,
                    _parentServiceMock.Object,
                    _checkServiceMock.Object,
                    _configMock.Object,
                    _searchSchoolsUseCaseMock.Object,
                    _loadParentDetailsUseCaseMock.Object,
                    _createUserUseCaseMock.Object,
                    _processParentDetailsUseCaseMock.Object,
                    _loadParentNassDetailsUseCaseMock.Object,
                    _loadParentLoaderUseCaseMock.Object,
                    _parentSignInUseCaseMock.Object,
                    _enterChildDetailsUseCaseMock.Object,
                    _processChildDetailsUseCaseMock.Object
                );
            }

            void SetUpSessionData()
            {
                _sessionMock = new Mock<ISession>();
                var sessionStorage = new Dictionary<string, byte[]>();

                _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                                .Callback<string, byte[]>((key, value) => sessionStorage[key] = value);

                _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                            .Returns((string key, out byte[] value) =>
                            {
                                var result = sessionStorage.TryGetValue(key, out var storedValue);
                                value = storedValue;
                                return result;
                            });
            }

            void SetUpHTTPContext()
            {
                _httpContext = new Mock<HttpContext>();
                _httpContext.Setup(ctx => ctx.Session).Returns(_sessionMock.Object);
                _sut.ControllerContext.HttpContext = _httpContext.Object;
            }

            void SetUpServiceMocks()
            {
                _checkServiceMock.Setup(s => s.GetStatus(It.IsAny<CheckEligibilityResponse>()))
                    .ReturnsAsync(_eligibilityStatusResponse);

                _checkServiceMock.Setup(s => s.PostCheck(It.IsAny<CheckEligibilityRequest_Fsm>()))
                    .ReturnsAsync(_eligibilityResponse);

                _parentServiceMock.Setup(s => s.GetSchool(It.IsAny<string>()))
                    .ReturnsAsync(_schoolSearchResponse);

                _parentServiceMock.Setup(s => s.PostApplication_Fsm(It.IsAny<ApplicationRequest>()))
                    .ReturnsAsync(_applicationSaveItemResponse);

                _parentServiceMock.Setup(s => s.CreateUser(It.IsAny<UserCreateRequest>()))
                    .ReturnsAsync(new UserSaveItemResponse { Data = "defaultUserId" });

                
                _enterChildDetailsUseCaseMock
                    .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<bool?>()))
                    .ReturnsAsync(_defaultChildren);
            }
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public async Task Given_EnterDetails_When_LoadingPageFromTempData_Should_BeAbleToLoadEnterDetailsPage()
        {
            // Arrange
            _sut.TempData["ParentDetails"] = JsonConvert.SerializeObject(_parent);
            var expectedViewModel = new LoadParentDetailsViewModel { Parent = _parent };
            _loadParentDetailsUseCaseMock
                .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedViewModel);

            // Act
            var result = await _sut.Enter_Details();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<Parent>();
            var model = viewResult.Model as Parent;
            model.Should().BeEquivalentTo(_parent);
        }

        [Test]
        public async Task Given_EnterDetails_When_LoadingPage_Should_LoadEnterDetailsPage()
        {
            // Arrange
            var expectedViewModel = new LoadParentDetailsViewModel();
            _loadParentDetailsUseCaseMock
                .Setup(x => x.ExecuteAsync(null, null))
                .ReturnsAsync(expectedViewModel);

            // Act
            var result = await _sut.Enter_Details();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public async Task Given_EnterDetails_When_ValidDataProvided_Should_RedirectToLoaderPage()
        {
            // Arrange
            _processParentDetailsUseCaseMock
                .Setup(x => x.ExecuteAsync(It.IsAny<Parent>(), It.IsAny<ISession>()))
                .ReturnsAsync((true, new CheckEligibilityResponse(), "Loader"));

            // Act
            var result = await _sut.Enter_Details(_parent);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.ActionName.Should().Be("Loader");
        }


        [Test]
        public async Task Given_EnterDetails_When_ValidDataProvided_Should_SetSessionData()
        {
            // Arrange
            var expectedDob = new DateOnly(1990, 01, 01).ToString("yyyy-MM-dd");
            var sessionStorage = new Dictionary<string, byte[]>();

            _sessionMock
                .Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) => sessionStorage[key] = value);

            _sessionMock
                .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) =>
                {
                    var result = sessionStorage.TryGetValue(key, out var storedValue);
                    value = storedValue;
                    return result;
                });

            _processParentDetailsUseCaseMock
                .Setup(x => x.ExecuteAsync(It.IsAny<Parent>(), It.IsAny<ISession>()))
                .Callback<Parent, ISession>((parent, session) =>
                {
                    session.Set("ParentFirstName", Encoding.UTF8.GetBytes(parent.FirstName));
                    session.Set("ParentLastName", Encoding.UTF8.GetBytes(parent.LastName));
                    session.Set("ParentDOB", Encoding.UTF8.GetBytes(expectedDob));
                    session.Set("ParentNINO", Encoding.UTF8.GetBytes(parent.NationalInsuranceNumber));
                })
                .ReturnsAsync((true, new CheckEligibilityResponse(), "Loader"));

            // Act
            await _sut.Enter_Details(_parent);

            // Assert
            Encoding.UTF8.GetString(sessionStorage["ParentFirstName"]).Should().Be(_parent.FirstName);
            Encoding.UTF8.GetString(sessionStorage["ParentLastName"]).Should().Be(_parent.LastName);
            Encoding.UTF8.GetString(sessionStorage["ParentDOB"]).Should().Be(expectedDob);
            Encoding.UTF8.GetString(sessionStorage["ParentNINO"]).Should().Be(_parent.NationalInsuranceNumber);
        }



        [Test]
        public async Task Given_EnterDetails_When_ModelStateIsInvalid_Should_ErrorsShouldBeInTempData()
        {

            _sut.ModelState.AddModelError("SomeErrorKey", "SomeErrorMessage");
            var error = _sut.ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList());
            _sut.TempData["Errors"] = JsonConvert.SerializeObject(error);

            // Act
            var result = _sut.Enter_Details();

            // Assert
            var errors = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(_sut.TempData["Errors"].ToString());
            errors["SomeErrorKey"][0].Should().Be("SomeErrorMessage");
        }

        [Test]
        public async Task Given_EnterDetails_When_ErrorsExistInTempData_Should_BeAddedToTheModelState()
        {
            // arrange
            _sut.ModelState.AddModelError("SomeErrorKey", "SomeErrorMessage");
            var error = _sut.ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList());
            _sut.TempData["Errors"] = JsonConvert.SerializeObject(error);

            // act
            var result = await _sut.Enter_Details(_parent);

            // assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Enter_Details");
        }

        [Test]
        public void Given_Nass_When_LoadingPage_Should_LoadNassPage()
        {
            // Arrange
            var mockParent = new Parent { FirstName = "Test", LastName = "Parent" };
            _sut.TempData["ParentDetails"] = JsonConvert.SerializeObject(mockParent);

            _loadParentNassDetailsUseCaseMock
                .Setup(x => x.ExecuteAsync(It.IsAny<string>()))
                .ReturnsAsync(mockParent);

            // Act
            var result = _sut.Nass();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<Parent>();
            var model = viewResult.Model as Parent;
            model.Should().BeEquivalentTo(mockParent);
        }

        [Test]
        public async Task Given_QueuedForProcessingStatus_When_LoaderIsCalled_Should_ReturnLoaderView()
        {
            // Arrange
            var response = new CheckEligibilityResponse
            {
                Data = new StatusValue { Status = "queuedForProcessing" }
            };
            var responseJson = JsonConvert.SerializeObject(response);
            _sut.TempData["Response"] = responseJson;

            _loadParentLoaderUseCaseMock
                .Setup(x => x.ExecuteAsync(responseJson))
                .ReturnsAsync(("Loader", null));

            // Act
            var result = await _sut.Loader();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Loader");
            _loadParentLoaderUseCaseMock.Verify(x => x.ExecuteAsync(responseJson), Times.Once);
        }

        [Test]
        public async Task Given_Nass_When_ValidDataProvided_Should_RedirectToNass()
        {
            // Arrange
            _parent.IsNinoSelected = false;
            _parent.IsNassSelected = false;
            _parent.NationalInsuranceNumber = null;
            _parent.NationalAsylumSeekerServiceNumber = "202405001";

            // Mock TempData
            var tempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
            _sut.TempData = tempData;

            // Mock ModelState
            _sut.ModelState.Clear();

            // Set up session and context properly using the existing session mock from SetUp
            var httpContext = new DefaultHttpContext();
            httpContext.Session = _sessionMock.Object; // Use the session mock we already set up
            _sut.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Mock ProcessParentDetailsUseCase to return expected values
            _processParentDetailsUseCaseMock
                .Setup(x => x.ExecuteAsync(
                    It.IsAny<Parent>(),
                    It.IsAny<ISession>()))
                .ReturnsAsync((false, null, "Nass"));

            // Act
            var result = await _sut.Enter_Details(_parent);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var actionResult = (RedirectToActionResult)result;
            actionResult.ActionName.Should().Be("Nass");
        }


        [TestCase("DwpError", "Outcome/Technical_Error")]
        [TestCase("eligible", "Outcome/Eligible")]
        [TestCase("notEligible", "Outcome/Not_Eligible")]
        [TestCase("parentNotFound", "Outcome/Not_Found")]
        [TestCase("queuedForProcessing", "Loader")]
        [TestCase("unknownStatus", "Outcome/Technical_Error")]
        public async Task Given_Loader_When_StatusProvided_Should_ReturnCorrectView(string status, string expectedView)
        {
            // Arrange
            var response = new CheckEligibilityResponse
            {
                Data = new StatusValue { Status = status }
            };
            var responseJson = JsonConvert.SerializeObject(response);
            _sut.TempData["Response"] = responseJson;

            _loadParentLoaderUseCaseMock
                .Setup(x => x.ExecuteAsync(responseJson))
                .ReturnsAsync((expectedView, null));

            // Act
            var result = await _sut.Loader();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be(expectedView);
            _loadParentLoaderUseCaseMock.Verify(x => x.ExecuteAsync(responseJson), Times.Once);
        }


        [Test]
        public async Task Given_CheckAnswers_When_LoadingPage_Should_LoadCheckAnswersPage()
        {
            // Act
            var result = _sut.Check_Answers();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public async Task Given_CheckAnswers_When_ValidDataProvided_Should_RedirectToApplicationSentPage()
        {
            // Arrange 

            _sut.ControllerContext.HttpContext.Session.SetString("CheckResult", (CheckYourEligibility.Domain.Enums.CheckEligibilityStatus.eligible.ToString()));
            _sut.TempData["FsmApplicationResponses"] = JsonConvert.SerializeObject(_applicationSaveItemResponse);

            // Act
            var result = _sut.Check_Answers(_fsmApplication);

            // Assert
            var actionResult = result.Result as RedirectToActionResult;
            actionResult.ActionName.Should().Be("Application_Sent");
            var applicationSaveItemResponses = JsonConvert.DeserializeObject<List<ApplicationSaveItemResponse>>(_sut.TempData["FsmApplicationResponses"] as string);
            applicationSaveItemResponses.First(x => x.Data.ParentFirstName == "Homer");
            applicationSaveItemResponses.First(x => x.Data.ChildFirstName == "Bart");
        }

        [Test]
        public async Task Given_ApplicationSent_When_LoadingPage_Should_LoadApplicationSentPage()
        {
            // Act
            var result = _sut.Application_Sent();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public async Task GetSchoolDetails_WhenQueryIsValid_ShouldReturnSchools()
        {
            // Arrange
            var query = "Test School";
            var schools = new List<Establishment>
        {
            new() { Name = "Test School 1" },
            new() { Name = "Test School 2" }
        };
            _searchSchoolsUseCaseMock.Setup(x => x.ExecuteAsync(query))
                .ReturnsAsync(schools);

            // Act
            var result = await _sut.SearchSchools(query);

            // Assert
            result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(schools);
            _searchSchoolsUseCaseMock.Verify(x => x.ExecuteAsync(query), Times.Once);
        }

        [Test]
        public async Task GetSchoolDetails_WhenQueryIsInvalid_ShouldReturnBadRequest()
        {
            // Arrange
            var query = "ab";
            _searchSchoolsUseCaseMock.Setup(x => x.ExecuteAsync(query))
                .ThrowsAsync(new ArgumentException("Query must be at least 3 characters long."));

            // Act
            var result = await _sut.SearchSchools(query);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("Query must be at least 3 characters long.");
        }

        [Test]
        public async Task GetSchoolDetails_WhenExceptionOccurs_ShouldReturnBadRequest()
        {
            // Arrange
            var query = "Test School";
            _searchSchoolsUseCaseMock.Setup(x => x.ExecuteAsync(query))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _sut.SearchSchools(query);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("An error occurred while searching for schools.");
        }

        





        [Test]
        public async Task Given_AddChild_When_AddingNewChild_Should_AddNewChildToEnterChildDetailsPageModel()
        {
            // Arrange
            _sut.TempData["IsChildAddOrRemove"] = true;
            _sut.TempData["ChildList"] = JsonConvert.SerializeObject(_children);

            // Act
            var result = _sut.Add_Child(_children);

            // Assert
            var children = JsonConvert.DeserializeObject<List<Child>>(_sut.TempData["ChildList"] as string).As<List<Child>>();

            children.Should().NotBeNull();
            children.Capacity.Should().Be(4);
            children.Last().FirstName.Should().BeNull();
            children.Last().LastName.Should().BeNull();
        }

        [Test]
        public async Task Given_RemoveChild_When_RemovingChildByIndex_Should_RemoveChildByIndexFromEnterChildDetailsPageModel()
        {
            // Arrange
            _sut.TempData["IsChildAddOrRemove"] = true;
            _sut.TempData["ChildList"] = JsonConvert.SerializeObject(_children);

            // Act
            var result = _sut.Remove_Child(_children, 2);

            // Assert
            var children = JsonConvert.DeserializeObject<List<Child>>(_sut.TempData["ChildList"] as string).As<List<Child>>();

            children.Should().NotBeNull();
            children.Count().Should().Be(2);
            children.Should().NotContain(x => x.FirstName == "Maggie");
        }

        [Test]
        public async Task Given_ChangeChildDetails_When_RedirectingToEnterChildDetailsPage_Should_PopulateExistingChildrensDetails()
        {
            // Arrange
            _sut.TempData["IsRedirect"] = true;
            _sut.TempData["FsmApplication"] = JsonConvert.SerializeObject(_fsmApplication);

            // Act
            int child = 0;
            var result = _sut.ChangeChildDetails();

            // Assert
            var viewResult = result as ViewResult;
            var children = viewResult.Model.As<Children>();
            children.Should().NotBeNull();
            children.ChildList.Count().Should().Be(3);
            viewResult.ViewName.Should().Be("Enter_Child_Details");
            children.ChildList[0].FirstName.Should().Be(_children.ChildList[0].FirstName);
            children.ChildList[1].FirstName.Should().Be(_children.ChildList[1].FirstName);
            children.ChildList[2].FirstName.Should().Be(_children.ChildList[2].FirstName);
        }



        [Test]
        public async Task Enter_Child_Details_When_LoadingPage_Should_LoadEnterChildDetailsPage()
        {
            // Arrange
            _enterChildDetailsUseCaseMock
                .Setup(x => x.ExecuteAsync(null, null))
                .ReturnsAsync(_defaultChildren);

            // Act
            var result = await _sut.Enter_Child_Details();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().NotBeNull();
            viewResult.Model.Should().BeAssignableTo<Children>();
            viewResult.Model.Should().BeEquivalentTo(_defaultChildren);

            // Verify the use case was called with expected parameters
            _enterChildDetailsUseCaseMock.Verify(
                x => x.ExecuteAsync(null, null),
                Times.Once);
        }

        [Test]
        public async Task Enter_Child_Details_When_IsChildAddOrRemove_Should_PopulateModelFromTempData()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    new Child { FirstName = "TestChild", LastName = "TestLastName" }
                }
            };
            _sut.TempData["IsChildAddOrRemove"] = true;
            _sut.TempData["ChildList"] = JsonConvert.SerializeObject(children.ChildList);

            _enterChildDetailsUseCaseMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<string>(s => s == JsonConvert.SerializeObject(children.ChildList)),
                    It.Is<bool?>(b => b == true)))
                .ReturnsAsync(children);

            // Act
            var result = await _sut.Enter_Child_Details();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<Children>().Subject;
            model.Should().BeEquivalentTo(children);

            _enterChildDetailsUseCaseMock.Verify(
                x => x.ExecuteAsync(
                    It.Is<string>(s => s == JsonConvert.SerializeObject(children.ChildList)),
                    It.Is<bool?>(b => b == true)),
                Times.Once);
        }

        [Test]
        public async Task Enter_Child_Details_When_NoTempData_Should_ReturnDefaultModel()
        {
            // Arrange
            _enterChildDetailsUseCaseMock
                .Setup(x => x.ExecuteAsync(null, null))
                .ReturnsAsync(new Children { ChildList = new List<Child> { new Child() } });

            // Act
            var result = await _sut.Enter_Child_Details();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<Children>().Subject;
            model.ChildList.Should().HaveCount(1);
            model.ChildList.First().Should().BeOfType<Child>();
        }

        [Test]
        public async Task Enter_Child_Details_WhenUseCaseThrows_ShouldPropagateException()
        {
            // Arrange
            _enterChildDetailsUseCaseMock
                .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<bool?>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await FluentActions.Invoking(() =>
                _sut.Enter_Child_Details())
                .Should().ThrowAsync<Exception>()
                .WithMessage("Test exception");
        }



        [Test]
        public async Task Given_AddChild_When_AddingMoreThan99Children_Should_CannotAddMoreThan99ChildrenToPagesModel()
        {
            // Arrange
            _sut.TempData["IsChildAddOrRemove"] = true;

            // create a list of 99 children (the max)
            var maxChildren = Enumerable.Range(1, 99).Select(x => new Child
            {
                FirstName = "Test",
                LastName = "Test",
                Day = "1",
                Month = "1",
                Year = "2010",
                School = _schools[0]
            }).ToList();

            _children.ChildList = maxChildren;
            _sut.TempData["ChildList"] = JsonConvert.SerializeObject(_children);

            // Act
            var result = _sut.Add_Child(_children);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Enter_Child_Details");
            _children.ChildList.Count.Should().Be(99);
        }

        [Test]
        public async Task SignIn_ShouldReturnChallengeResultWithCorrectSchemeAndProperties()
        {
            // Arrange
            var mockAuthProperties = new AuthenticationProperties
            {
                RedirectUri = "/Check/CreateUser"
            };
            mockAuthProperties.SetString("vector_of_trust", @"[""Cl""]");

            _parentSignInUseCaseMock
                .Setup(x => x.ExecuteAsync("/Check/CreateUser"))
                .ReturnsAsync(mockAuthProperties);

            // Act
            var result = await _sut.SignIn();

            // Assert
            result.Should().BeOfType<ChallengeResult>();
            var challengeResult = result as ChallengeResult;

            challengeResult.AuthenticationSchemes.Should().ContainSingle()
                .Which.Should().Be(OneLoginDefaults.AuthenticationScheme);

            challengeResult.Properties.Should().NotBeNull();
            challengeResult.Properties.RedirectUri.Should().Be("/Check/CreateUser");
            challengeResult.Properties.GetString("vector_of_trust").Should().Be(@"[""Cl""]");

            _parentSignInUseCaseMock.Verify(
                x => x.ExecuteAsync("/Check/CreateUser"),
                Times.Once);
        }


        [Test]
        public async Task Given_EnterDetails_When_UserHasSelectedToInputANass_Should_RedirectToNassPage()
        {
            // Arrange
            _parent.IsNinoSelected = false;
            _parent.NationalInsuranceNumber = null;

            _processParentDetailsUseCaseMock
                .Setup(x => x.ExecuteAsync(It.IsAny<Parent>(), It.IsAny<ISession>()))
                .ReturnsAsync((false, null, "Nass"));

            // Act
            var result = await _sut.Enter_Details(_parent);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Nass");
        }

        [Test]
        
        public async Task CreateUser_WhenSuccessful_ShouldSetSessionAndRedirect()
        {
            // Arrange
            var email = "test@example.com";
            var uniqueId = "unique123";
            var userId = "user123";

            var claims = new List<Claim>
    {
        new Claim("email", email),
        new Claim("sub", uniqueId)
    };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Use the existing session mock setup
            var httpContext = new DefaultHttpContext();
            httpContext.User = principal;
            httpContext.Session = _sessionMock.Object;

            _sut.ControllerContext.HttpContext = httpContext;

            _createUserUseCaseMock.Setup(x => x.ExecuteAsync(
                It.Is<string>(e => e == email),
                It.Is<string>(u => u == uniqueId)))
                .ReturnsAsync(userId);

            // Act
            var result = await _sut.CreateUser();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Enter_Child_Details");

            // Get the values from the session using the mocked methods
            Encoding.UTF8.GetString(_sessionMock.Object.Get("Email")).Should().Be(email);
            Encoding.UTF8.GetString(_sessionMock.Object.Get("UserId")).Should().Be(userId);
            _createUserUseCaseMock.Verify(x => x.ExecuteAsync(email, uniqueId), Times.Once);
        }

        [Test]
        public async Task CreateUser_WhenUseCaseThrowsException_ShouldReturnTechnicalError()
        {
            // Arrange
            var email = "test@example.com";
            var uniqueId = "unique123";

            var claims = new List<Claim>
    {
        new Claim("email", email),
        new Claim("sub", uniqueId)
    };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _sut.ControllerContext.HttpContext = new DefaultHttpContext();
            _sut.ControllerContext.HttpContext.User = principal;

            _createUserUseCaseMock.Setup(x => x.ExecuteAsync(
                It.Is<string>(e => e == email),
                It.Is<string>(u => u == uniqueId)))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _sut.CreateUser();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Outcome/Technical_Error");
            _createUserUseCaseMock.Verify(x => x.ExecuteAsync(email, uniqueId), Times.Once);
        }

        [Test]
        public async Task CreateUser_WhenClaimsAreMissing_ShouldReturnTechnicalError()
        {
            // Arrange
            _sut.ControllerContext.HttpContext = new DefaultHttpContext();
            _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await _sut.CreateUser();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Outcome/Technical_Error");
            _createUserUseCaseMock.Verify(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}

