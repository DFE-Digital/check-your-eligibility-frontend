using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Controllers;
using CheckYourEligibility_FrontEnd.Models;
using CheckYourEligibility_FrontEnd.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using ChildsSchool = CheckYourEligibility_FrontEnd.Models.School;
using School = CheckYourEligibility.Domain.Responses.School;

namespace CheckYourEligibility_Parent.Tests.Controllers
{
    [TestFixture]
    public class CheckControllerShould
    {
        // mocks
        private ILogger<CheckController> _loggerMock;
        private Mock<IEcsServiceParent> _serviceMock;
        private Mock<ISession> _sessionMock;
        private Mock<HttpContext> _httpContext;

        // check eligibility responses
        private CheckEligibilityResponse _eligibilityResponse;
        private CheckEligibilityStatusResponse _eligibilityStatusResponse;
        private SchoolSearchResponse _schoolSearchResponse;
        private ApplicationSaveItemResponse _applicationSaveItemResponse;

        // test data entities
        private FsmApplication _fsmApplication;
        private ChildsSchool[] _schools;
        private Parent _parent;
        private Children _children;

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
                _schools = [
                    new ChildsSchool()
                {
                    Name = "Springfield Elementary",
                    LA = "Springfield",
                    Postcode = "SP1 3LE",
                    URN = "10002"
                },
                new ChildsSchool()
                {
                    Name = "Springfield Nursery",
                    LA = "Springfield",
                    Postcode = "SP1 3NU",
                    URN = "10001"
                }
                ];

                _parent = new Parent()
                {
                    FirstName = "Homer",
                    LastName = "Simpson",
                    Day = 1,
                    Month = 1,
                    Year = 1990,
                    NationalInsuranceNumber = "AB123456C",
                    NationalAsylumSeekerServiceNumber = null,
                    IsNassSelected = false
                };

                _children = new Children()
                {
                    ChildList =
                    [
                        new Child()
                    {
                        FirstName = "Bart",
                        LastName = "Simpson",
                        Day = 1,
                        Month = 1,
                        Year = 2015,
                        School = _schools[0]
                    },
                    new Child()
                    {
                        FirstName = "Lisa",
                        LastName = "Simpson",
                        Day = 1,
                        Month = 1,
                        Year = 2018,
                        School = _schools[0]
                    },
                    new Child()
                    {
                        FirstName = "Maggie",
                        LastName = "Simpson",
                        Day = 1,
                        Month = 1,
                        Year = 2020,
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
                        // Set the properties of the CheckEligibilityResponseData object
                        Status = "queuedForProcessing"
                    },
                    Links = new CheckEligibilityResponseLinks
                    {
                        // Set the properties of the CheckEligibilityResponseLinks object
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

                _schoolSearchResponse = new SchoolSearchResponse()
                {
                    Data = [new School()
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
                }]
                };

                _applicationSaveItemResponse = new ApplicationSaveItemResponse()
                {
                    Data = new ApplicationSave()
                    {
                        ParentFirstName = _fsmApplication.ParentFirstName,
                        ParentLastName = _fsmApplication.ParentLastName,
                        ParentDateOfBirth = _fsmApplication.ParentDateOfBirth,
                        ParentNationalInsuranceNumber = _fsmApplication.ParentNino,
                        ChildFirstName = _fsmApplication.Children.ChildList[0].FirstName,
                        ChildLastName = _fsmApplication.Children.ChildList[0].LastName,
                        ChildDateOfBirth = new DateOnly(_fsmApplication.Children.ChildList[0].Year.Value, _fsmApplication.Children.ChildList[0].Month.Value, _fsmApplication.Children.ChildList[0].Day.Value).ToString("dd/MM/yyyy"),
                        ParentNationalAsylumSeekerServiceNumber = _fsmApplication.ParentNass,
                        Id = "",
                        LocalAuthority = 10002,
                        Reference = "",
                        School = 10002
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
                _serviceMock = new Mock<IEcsServiceParent>();
                _loggerMock = Mock.Of<ILogger<CheckController>>();
                _sut = new CheckController(_loggerMock, _serviceMock.Object);
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
                _serviceMock.Setup(s => s.GetStatus(It.IsAny<CheckEligibilityResponse>()))
                    .ReturnsAsync(_eligibilityStatusResponse);

                _serviceMock.Setup(s => s.PostCheck(It.IsAny<CheckEligibilityRequest>()))
                            .ReturnsAsync(_eligibilityResponse);

                _serviceMock.Setup(s => s.GetSchool(It.IsAny<string>()))
                            .ReturnsAsync(_schoolSearchResponse);

                _serviceMock.Setup(s => s.PostApplication(It.IsAny<ApplicationRequest>()))
                            .ReturnsAsync(_applicationSaveItemResponse);
            }
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public async Task BeAbleToLoadTheEnterDetailsPage_FromTempData()
        {
            // Arrange
            _sut.TempData["ParentDetails"] = JsonConvert.SerializeObject(_parent);

            // Act
            var result = _sut.Enter_Details();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<Parent>();
            var model = viewResult.Model as Parent;
            model.Should().BeEquivalentTo(_parent);
        }

        [Test]
        public async Task LoadTheEnterDetailsPage()
        {
            // Act
            var result = _sut.Enter_Details();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().Be(null);
        }

        [Test]
        public async Task RedirectToLoaderPageFromEnterDetailsPage_GivenValidData()
        {
            // Act
            var result = await _sut.Enter_Details(_parent);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.ActionName.Should().Be("Loader");
        }

        [Test]
        public async Task SetSessionData_GivenValidData()
        {
            // Arrange
            var expectedDob = new DateOnly(1990, 01, 01).ToString();

            // Act
            _ = await _sut.Enter_Details(_parent);

            // Assert
            _sut.ControllerContext.HttpContext.Session.GetString("ParentFirstName").Should().Be(_parent.FirstName);
            _sut.ControllerContext.HttpContext.Session.GetString("ParentLastName").Should().Be(_parent.LastName);
            _sut.ControllerContext.HttpContext.Session.GetString("ParentDOB").Should().Be(expectedDob);
            _sut.ControllerContext.HttpContext.Session.GetString("ParentNINO").Should().Be(_parent.NationalInsuranceNumber);
        }

        [Test]
        public async Task LoadTheNassPage()
        {
            // Act
            var result = _sut.Nass();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<Parent>();
            var model = viewResult.Model as Parent;
            model.Should().BeEquivalentTo(new Parent());
        }

        [Test]
        public async Task RedirectToLoaderPage_FromNassPage_GivenValidData()
        {
            // Arrange
            _parent.IsNassSelected = true;
            _parent.NationalInsuranceNumber = null;
            _parent.NationalAsylumSeekerServiceNumber = "202405001";

            // Act
            var result = _sut.Nass(_parent);

            // Assert
            result.Should().BeOfType<Task<IActionResult>>();

            var actionResult = (RedirectToActionResult)result.Result;
            actionResult.ActionName.Should().Be("Loader");
        }

        [Test]
        public async Task BeAbleToRedirectToThe_CouldNotCheckPage_FromNassPage_()
        {
            // Arrange
            _parent.IsNassSelected = true;
            _parent.NationalInsuranceNumber = null;

            // Act
            var result = _sut.Nass(_parent);

            // Assert
            result.Should().BeOfType<Task<IActionResult>>();

            var viewResult = (ViewResult)result.Result;
            viewResult.ViewName.Should().Be("Outcome/Could_Not_Check");
        }

        [Test]
        public async Task LoadTheLoaderPage()
        {
            // Act
            var result = _sut.Loader();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public async Task LoadTheCheckAnswersPage()
        {
            // Act
            var result = _sut.Check_Answers();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public async Task RedirectToApplicationSentPage_AfterCheckAnswers()
        {
            // Arrange 
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
        public async Task LoadTheApplicationSentPage()
        {
            // Act
            var result = _sut.Application_Sent();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public async Task NotReturnSchoolsData_GivenQueryIsLessThan3Chars()
        {
            // Arrange
            var query = "ab";

            // Act
            var result = _sut.GetSchoolDetails(query);

            // Assert
            result.Should().BeOfType<Task<IActionResult>>();
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task ReturnSchoolsData_GivenValidQuery()
        {
            // Arrange
            var query = "Test";
            var expectedName = _schoolSearchResponse.Data.First().Name;
            var expectedPostcode = _schoolSearchResponse.Data.First().Postcode;

            // Act
            var result = _sut.GetSchoolDetails(query);

            // Assert
            result.Result.Should().BeOfType<JsonResult>();
            var jsonResult = result.Result as JsonResult;

            foreach (var school in jsonResult.Value as dynamic)
            {
                ((string)school.Name).Should().Be(expectedName);
                ((string)school.Postcode).Should().Be(expectedPostcode);
            }
        }

        [Test]
        public async Task NotReturnAnySchoolDetails_WhenNoSchoolExistsFromQuery()
        {
            // Arrange
            var query = "Not a real school";
            _serviceMock.Setup(x => x.GetSchool(query)).ReturnsAsync(_schoolSearchResponse = null);

            // Act
            var result = _sut.GetSchoolDetails(query);

            // Assert
            result.Result.Should().Be(null);
        }

        [TestCase("eligible", "Outcome/Eligible")]
        [TestCase("notEligible", "Outcome/Not_Eligible")]
        [TestCase("parentNotFound", "Outcome/Not_Found")]
        [TestCase("queuedForProcessing", "Outcome/Default")]
        [TestCase("notARealStatus", "Outcome/Default")]
        public async Task ReturnOutcomePageBasedOnEligibilityResponse_FromPollStatus(string status, string expected)
        {
            // Arrange
            _eligibilityStatusResponse.Data.Status = status;
            _sut.TempData["Response"] = JsonConvert.SerializeObject(_eligibilityResponse);

            // Act
            var result = await _sut.Poll_Status();

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be(expected);
        }

        [Test]
        public async Task AddNewChildTo_EnterChildDetailsPageModel()
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
        public async Task RemoveChildByIndexFrom_EnterChildDetailsPageModel()
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
        public async Task RedirectToEnterChildDetailsPage_AndPopulateExistingChildrensDetails()
        {
            // Arrange
            _sut.TempData["IsRedirect"] = true;
            _sut.TempData["FsmApplication"] = JsonConvert.SerializeObject(_fsmApplication);

            // Act
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
    }
}