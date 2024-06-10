﻿using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;

namespace CheckYourEligibility_FrontEnd.Services.Tests.Parent
{
    public class EcsServiceParentShould
    {
        private Mock<ILoggerFactory> _loggerFactoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IConfiguration> _configMock;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private EcsServiceParentTest _sut;

        [SetUp]
        public void Setup()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger>();
            _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(x => x["Api:AuthorisationUsername"]).Returns("SomeValue");
            _configMock.Setup(x => x["Api:AuthorisationPassword"]).Returns("SomeValue");
            _configMock.Setup(x => x["Api:AuthorisationEmail"]).Returns("SomeValue");

            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost:7000")
            };

            _sut = new EcsServiceParentTest(_loggerFactoryMock.Object, _httpClient, _configMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }

        [Test]
        public async Task GetSchool_ShouldReturnSchoolSearchResponse()
        {
            // Arrange
            var query = "Test";
            var responseContent = new SchoolSearchResponse();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(responseContent))
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _sut.GetSchool(query);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseContent);
        }

        [Test]
        public async Task PostApplication_ShouldReturnApplicationSaveItemResponse()
        {
            // Arrange
            var requestBody = new ApplicationRequest();
            var responseContent = new ApplicationSaveItemResponse();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(responseContent))
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _sut.PostApplication(requestBody);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseContent);
        }

        [Test]
        public async Task GetStatus_ShouldReturnCheckEligibilityStatusResponse()
        {
            // Arrange
            var responseBody = new CheckEligibilityResponse
            {
                Links = new CheckEligibilityResponseLinks { Get_EligibilityCheck = "EligibilityCheckLink" }
            };
            var responseContent = new CheckEligibilityStatusResponse();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(responseContent))
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _sut.GetStatus(responseBody);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseContent);
        }

        [Test]
        public async Task PostCheck_ShouldReturnCheckEligibilityResponse()
        {
            // Arrange
            var requestBody = new CheckEligibilityRequest();
            var responseContent = new CheckEligibilityResponse();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(responseContent))
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _sut.PostCheck(requestBody);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseContent);
        }

        [Test]
        public async Task GetSchool_ShouldReturnNullAndLogAPIError()
        {
            // Arrange
            var query = "Test";
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _sut.GetSchool(query);

            // Assert
            result.Data.Should().BeNull();
            _sut.apiErrorCount.Should().Be(1);

        }

        [Test]
        public async Task PostApplication_ShouldReturnNullAndLogAPIError()
        {
            // Arrange
            var requestBody = new ApplicationRequest();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _sut.PostApplication(requestBody);

            // Assert
            result.Data.Should().BeNull();
            result.Links.Should().BeNull();
            _sut.apiErrorCount.Should().Be(1);
        }

        [Test]
        public void GetStatus_ShouldThrowArgumentNullException_OnNullInput()
        {
            // Act
            var result = async () => await _sut.GetStatus(null);

            // Assert
            result.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task PostCheck_ShouldLogApiErrorAndReturnNullResultOn401Error()
        {
            // Arrange
            var requestBody = new CheckEligibilityRequest();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = _sut.PostCheck(requestBody);

            // Assert
            result.Result.Should().BeNull();
            _sut.apiErrorCount.Should().Be(1);
        }
    }
}
