using CheckYourEligibility_FrontEnd.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckYourEligibility_Parent.Tests.Controllers
{
    [TestFixture]
    internal class HomeControllerTests
    {

        private HomeController _sut;
        private ILogger<HomeController> _logger;

        [SetUp]
        public void SetUp()
        {
            _logger = Mock.Of<ILogger<HomeController>>();
            _sut = new HomeController(_logger);
        }

        [TearDown]
        public void TearDown() 
        {
            _sut.Dispose();
        }

        public HomeControllerTests()
        {
                
        }

        [Test]
        public void Given_Accessibility_LoadsWithEmptyModel()
        {
            // Arrange
            var controller = new HomeController(_logger);

            // Act
            var result = controller.Accessibility();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Accessibility");
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public void Given_Privacy_LoadsWithEmptyModel()
        {
            // Arrange
            var controller = new HomeController(_logger);

            // Act
            var result = controller.Privacy();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Privacy");
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public void Given_Cookies_LoadsWithEmptyModel()
        {
            // Arrange
            var controller = new HomeController(_logger);

            // Act
            var result = controller.Cookies();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Cookies");
            viewResult.Model.Should().BeNull();
        }
    }
}
