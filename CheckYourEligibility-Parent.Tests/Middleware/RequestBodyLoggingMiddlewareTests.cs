using CheckYourEligibility_FrontEnd.Middleware;
using CheckYourEligibility_FrontEnd.Models;
using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CheckYourEligibility_Parent.Tests.Middleware
{
   

    public class RequestBodyLoggingMiddlewareTests
    {
        private RequestBodyLoggingMiddleware _middleware;
        private Mock<ILogger<RequestBodyLoggingMiddleware>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<RequestBodyLoggingMiddleware>>();
            _middleware = new RequestBodyLoggingMiddleware(
                next: (HttpContext context) => Task.CompletedTask,
                logger: _mockLogger.Object
            );
        }

        [Test]
        public async Task Invoke_ShouldLogRequestBody_WhenContentTypeIsJson()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.ContentType = "application/json";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"test\":\"value\"}"));
            context.Request.ContentLength = 16;

            // Act
            await _middleware.Invoke(context);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Request Body: {\"test\":\"value\"}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }
    }

    
}