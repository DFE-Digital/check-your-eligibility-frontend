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
            
        public class ResponseBodyLoggingMiddlewareTests
    {
        private ResponseBodyLoggingMiddleware _middleware;
        private Mock<ILogger<ResponseBodyLoggingMiddleware>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ResponseBodyLoggingMiddleware>>();
            _middleware = new ResponseBodyLoggingMiddleware(
                next: async (HttpContext context) =>
                {
                    await context.Response.WriteAsync("{\"response\":\"test\"}");
                },
                logger: _mockLogger.Object
            );
        }

        [Test]
        public async Task Invoke_ShouldLogResponseBody()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await _middleware.Invoke(context);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response Body: {\"response\":\"test\"}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }
    }
}