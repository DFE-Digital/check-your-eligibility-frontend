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

namespace CheckYourEligibility_Parent.Tests.Controllers
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

        public ApplicationResponse CreateApplicationResponse(string status)
        {
            return new ApplicationResponse
            {
                Id = "12345",
                Reference = "123456789",
                School = new ApplicationResponse.ApplicationSchool
                {
                    Id = 1,
                    Name = "Hollinswood",
                    LocalAuthority = new ApplicationResponse.ApplicationSchool.SchoolLocalAuthority
                    {
                        Id = 50,
                        Name = "Telford and Wrekin"
                    }
                },
                ParentFirstName = "Tim",
                ParentLastName = "Smith",
                ParentNationalInsuranceNumber = "AB123456C",
                ParentNationalAsylumSeekerServiceNumber = null,
                ParentDateOfBirth = "1990-01-01",
                ChildFirstName = "Timmy",
                ChildLastName = "Smith",
                ChildDateOfBirth = "2010-01-01",
                Status = status ,
                User = new ApplicationResponse.ApplicationUser
                {
                    UserID = "9876",
                    Email = "Test@User.com",
                    Reference = "12345",
                },
                Created = DateTime.Now,
                CheckOutcome = new ApplicationResponse.ApplicationHash
                {
                    Outcome = "Entitled"
                }

            };
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
        public async Task Given_Application_Search_Can_Filter_Based_On_Status()
        {
            //arrange
            var statuses = new[] { "Entitled", "Receiving", "EvidenceNeeded", "SentForReview", "ReviewedEntitled", "ReviewedNotEntitled" };
            var applicationResponses = new List<ApplicationResponse>();

            foreach (var status in statuses)
            {
                applicationResponses.Add(CreateApplicationResponse(status));
            }
            var applicationSearchResponse = new ApplicationSearchResponse
            {
                Data = applicationResponses
            };

            var request = new ApplicationSearch
            {
                Status = CheckYourEligibility.Domain.Enums.ApplicationStatus.Entitled
            };
            ////arrange
            
            //var response = _fixture.Create<ApplicationSearchResponse>();

            _adminServiceMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
                   .ReturnsAsync(applicationSearchResponse);

            ////act
            var result = await _sut.Results(request);

            ////assert
            result.Should().BeOfType<ViewResult>();

            //var viewResult = result as ViewResult;
            //viewResult.Model.Should().BeAssignableTo<ApplicationSearchResponse>();

            //var model = viewResult.Model as ApplicationSearchResponse;
            //model.Should().NotBeNull();
            //model.Should().BeEquivalentTo(response);

        }
        // Set up test response object

    }
}
