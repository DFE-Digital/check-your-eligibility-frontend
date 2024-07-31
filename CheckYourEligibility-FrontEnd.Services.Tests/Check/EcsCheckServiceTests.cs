using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services.Tests.Parent;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;

namespace CheckYourEligibility_FrontEnd.Services.Tests.Check
{
    internal class EcsCheckServiceTests
    {
        private Mock<ILoggerFactory> _loggerFactoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IConfiguration> _configMock;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private DerivedCheckService _sut;

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

            _sut = new DerivedCheckService(_loggerFactoryMock.Object, _httpClient, _configMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }


        [Test]
        public async Task Given_GetStatus_When_CalledWithValidResponse_Should_ReturnCheckEligibilityStatusResponse()
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
        public async Task Given_PostCheck_When_CalledWithValidRequest_Should_ReturnCheckEligibilityResponse()
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
        public void Given_GetStatus_When_CalledWithNullInput_Should_ThrowArgumentNullException()
        {
            // Act
            var result = async () => await _sut.GetStatus(null);


            // Assert
            var resultAsResponse = result.As<Task<CheckEligibilityStatusResponse>>();
            resultAsResponse.Should().BeNull();

            result.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task Given_GetStatus_When_GivenInAnInvalidValue_ShouldReturnNull()
        {
            // Arrange

            // Assert
            var result = _sut.GetStatus(new CheckEligibilityResponse()
            {
                Data = new StatusValue()
                {
                    Status = "unknown"
                },
                Links = new CheckEligibilityResponseLinks()
                {
                    Get_EligibilityCheck = "link"
                }
            });

            // Act
            result.Result.Should().Be(null);

        }

        [Test]
        public async Task Given_PostCheck_When_ApiReturnsUnauthorized_Should_LogApiErrorAndReturnNullResult()
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


        [Test]
        public async Task Given_GetStatus_When_ApiReturnsUnauthorized_Should_LogApiErrorAndThrowExecption()
        {
            // Arrange
            var responseBody = new CheckEligibilityResponse
            {
                Links = new CheckEligibilityResponseLinks { Get_EligibilityCheck = "EligibilityCheckLink" }
            };
            var responseContent = new CheckEligibilityStatusResponse();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent(JsonConvert.SerializeObject(responseContent))
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = _sut.GetStatus(responseBody);

            // Assert
            result.Result.Should().BeNull();
            _sut.apiErrorCount.Should().Be(1);
        }
    }
}
