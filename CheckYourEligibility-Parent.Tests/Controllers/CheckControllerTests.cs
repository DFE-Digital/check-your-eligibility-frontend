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
using Microsoft.Extensions.Configuration;
using Azure.Core;

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
                _schools = new[]
                {
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

                _schoolSearchResponse = new SchoolSearchResponse()
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
                        School = new ApplicationResponse.ApplicationSchool { Id = 10002, LocalAuthority = new ApplicationResponse.ApplicationSchool.SchoolLocalAuthority { Id = 123 } },
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
                _sut = new CheckController(_loggerMock, _parentServiceMock.Object, _checkServiceMock.Object, _configMock.Object);
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

                _checkServiceMock.Setup(s => s.PostCheck(It.IsAny<CheckEligibilityRequest>()))
                            .ReturnsAsync(_eligibilityResponse);

                _parentServiceMock.Setup(s => s.GetSchool(It.IsAny<string>()))
                            .ReturnsAsync(_schoolSearchResponse);

                _parentServiceMock.Setup(s => s.PostApplication(It.IsAny<ApplicationRequest>()))
                            .ReturnsAsync(_applicationSaveItemResponse);
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
        public async Task Given_EnterDetails_When_LoadingPage_Should_LoadEnterDetailsPage()
        {
            // Act
            var result = _sut.Enter_Details();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().Be(null);
        }

        [Test]
        public async Task Given_EnterDetails_When_ValidDataProvided_Should_RedirectToLoaderPage()
        {
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

            // Act
            _ = await _sut.Enter_Details(_parent);

            // Assert
            _sut.ControllerContext.HttpContext.Session.GetString("ParentFirstName").Should().Be(_parent.FirstName);
            _sut.ControllerContext.HttpContext.Session.GetString("ParentLastName").Should().Be(_parent.LastName);
            _sut.ControllerContext.HttpContext.Session.GetString("ParentDOB").Should().Be(expectedDob);
            _sut.ControllerContext.HttpContext.Session.GetString("ParentNINO").Should().Be(_parent.NationalInsuranceNumber);
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
        public async Task Given_Nass_When_LoadingPage_Should_LoadNassPage()
        {
            //arrange
            var request = new Parent();
            _sut.TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
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
        public async Task Given_Nass_When_ValidDataProvided_Should_RedirectToNass()
        {
            // Arrange
            _parent.IsNinoSelected = false;
            _parent.NationalInsuranceNumber = null;
            _parent.NationalAsylumSeekerServiceNumber = "202405001";

            // Act
            var result = _sut.Enter_Details(_parent);

            // Assert
            result.Should().BeOfType<Task<IActionResult>>();

            var actionResult = (RedirectToActionResult)result.Result;
            actionResult.ActionName.Should().Be("Nass");
        }


        [Test]
        public async Task Given_Loader_When_LoadingPage_Should_LoadLoaderPage()
        {
            // Act
            var result = _sut.Loader();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeNull();
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

            _sut.SetSessionCheckResult(CheckYourEligibility.Domain.Enums.CheckEligibilityStatus.eligible.ToString());
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
        public async Task Given_GetSchoolDetails_When_QueryIsLessThan3Chars_Should_NotReturnSchoolsData()
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
        public async Task Given_GetSchoolDetails_When_ValidQueryProvided_Should_ReturnSchoolsData()
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
        public async Task Given_GetSchoolDetails_When_NoSchoolExistsFromQuery_Should_NotReturnAnySchoolDetails()
        {
            // Arrange
            var query = "Not a real school";
            _parentServiceMock.Setup(x => x.GetSchool(query)).ReturnsAsync(_schoolSearchResponse = null);

            // Act
            var result = _sut.GetSchoolDetails(query);

            // Assert
            result.Result.Should().BeOfType<JsonResult>();
        }

        [TestCase("eligible", "Outcome/Eligible", typeof(PartialViewResult), false)]
        [TestCase("eligible", "Outcome/Eligible", typeof(ViewResult), true)]
        [TestCase("notEligible", "Outcome/Not_Eligible", typeof(PartialViewResult), false)]
        [TestCase("notEligible", "Outcome/Not_Eligible", typeof(ViewResult), true)]
        [TestCase("parentNotFound", "Outcome/Not_Found", typeof(PartialViewResult), false)]
        [TestCase("parentNotFound", "Outcome/Not_Found", typeof(ViewResult), true)]
        [TestCase("DwpError", "Outcome/Technical_Error", typeof(PartialViewResult), false)]
        [TestCase("DwpError", "Outcome/Technical_Error", typeof(ViewResult), true)]
        [TestCase("queuedForProcessing", "Loader", typeof(JsonResult), false)]
        [TestCase("queuedForProcessing", "Loader", typeof(RedirectToActionResult), true)]
        [TestCase("notARealStatus", "Outcome/Technical_Error", typeof(JsonResult), false)]
        [TestCase("notARealStatus", "Outcome/Technical_Error", typeof(ViewResult), true)]
        public async Task Given_PollStatus_When_EligibilityResponseProvided_Should_ReturnOutcomePageBasedOnEligibilityResponse(
    string status, string expectedView, Type expectedType, bool jsDisabled)
        {
            // Arrange
            var checkEligibilityResponse = new CheckEligibilityResponse
            {
                Data = new StatusValue { Status = status }
            };

            _sut.TempData["Response"] = JsonConvert.SerializeObject(checkEligibilityResponse);

            var checkEligibilityStatusResponse = new CheckEligibilityStatusResponse
            {
                Data = new StatusValue { Status = status }
            };

            _checkServiceMock
                .Setup(x => x.GetStatus(It.IsAny<CheckEligibilityResponse>()))
                .ReturnsAsync(checkEligibilityStatusResponse);

            // Act
            var result = await _sut.Poll_Status(jsDisabled);

            // Assert
            result.Should().BeOfType(expectedType);

            if (expectedType == typeof(ViewResult))
            {
                var viewResult = result as ViewResult;
                viewResult.Should().NotBeNull();
                viewResult.ViewName.Should().Be(expectedView);
            }
            else if (expectedType == typeof(PartialViewResult))
            {
                var partialViewResult = result as PartialViewResult;
                partialViewResult.Should().NotBeNull();
                partialViewResult.ViewName.Should().Be(expectedView);
            }
            else if (expectedType == typeof(RedirectToActionResult))
            {
                var redirectResult = result as RedirectToActionResult;
                redirectResult.Should().NotBeNull();
                redirectResult.ActionName.Should().Be(expectedView);
            }
            else if (expectedType == typeof(JsonResult))
            {
                var jsonResult = result as JsonResult;
                jsonResult.Should().NotBeNull();
            }
            else
            {
                Assert.Fail("Unexpected result type");
            }
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
        public async Task Given_EnterChildDetails_When_LoadingPage_Should_LoadEnterChildDetailsPage()
        {
            // Act
            var result = _sut.Enter_Child_Details();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeAssignableTo<Children>();
        }

        [Test]
        public async Task Given_EnterChildDetails_When_SubmittedWithData_Should_LoadCheckAnswersPage()
        {
            // Arrange
            _sut.TempData["FsmApplication"] = JsonConvert.SerializeObject(_fsmApplication);

            // Act
            var result = _sut.Enter_Child_Details(_children);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Check_Answers");
            ; viewResult.Model.Should().BeAssignableTo<FsmApplication>();
        }

        [Test]
        public async Task Given_EnterChildDetails_When_NavigatedFromARedirect_Should_LoadWithChildrenDetailsInModel()
        {
            // arrange
            _sut.TempData["FsmApplication"] = JsonConvert.SerializeObject(_fsmApplication);
            _sut.TempData["IsRedirect"] = true;

            // act
            var result = _sut.Enter_Child_Details(_children);

            // assert
            var viewResult = result as ViewResult;
            viewResult.Model.Should().Be(_children);
        }

        [Test]
        public async Task Given_EnterChildDetails_When_IsChildAddOrRemove_Should_PopulateModelFromTempData()
        {
            // arrange
            _sut.TempData["IsChildAddOrRemove"] = true;
            _sut.TempData["ChildList"] = JsonConvert.SerializeObject(_children.ChildList);

            // act
            var result = _sut.Enter_Child_Details();

            // assert
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<Children>();
            var actionResult = result as ActionResult;


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
        public async Task Given_EnterDetails_When_UserHasSelectedToInputANass_Should_RedirectToNassPage()
        {
            // Arrange
            _parent.IsNinoSelected = false;
            _parent.NationalInsuranceNumber = null;

            // Act
            var result = _sut.Enter_Details(_parent);

            // Assert
            var redirectResult = result.Result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Nass");
        }

        [Test]
        public async Task Given_CheckController_When_LoggerIsNull_Should_ReturnArgumentNullException()
        {
            try
            {
                _sut = new CheckController(null, _parentServiceMock.Object, _checkServiceMock.Object, _configMock.Object);
            }
            catch (ArgumentNullException ex)
            {
                ex.Should().NotBeNull();
            }
        }

        [Test]
        public async Task Given_CheckController_When_ParentServiceIsNull_Should_ReturnArgumentNullException()
        {
            try
            {
                _sut = new CheckController(_loggerMock, null, _checkServiceMock.Object, _configMock.Object);
            }
            catch (ArgumentNullException ex)
            {
                ex.Should().NotBeNull();
            }
        }

        [Test]
        public async Task Given_CheckController_When_CheckServiceIsNull_Should_ReturnArgumentNullException()
        {
            try
            {
                _sut = new CheckController(_loggerMock, _parentServiceMock.Object, null, _configMock.Object);
            }
            catch (ArgumentNullException ex)
            {
                ex.Should().NotBeNull();
            }
        }

        [Test]
        public async Task Given_Nass_When_BadNassSubmitted_Should_ReturnNassPageWithErrors()
        {
            // Arrange
            _sut.ModelState.AddModelError("TestError", "TestErrorMessage");

            // Act
            var result = _sut.Enter_Details(_parent);

            // Assert
            _sut.ModelState.IsValid.Should().BeFalse();

            var viewResult = result.Result as RedirectToActionResult;
            viewResult.ActionName = "Enter_Details";
        }
    }
}

