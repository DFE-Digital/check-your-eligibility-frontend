using CheckYourEligibility_FrontEnd.Middleware;
using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace CheckYourEligibility_Parent.Tests.Middleware
{
    public class ExceptionLoggingMiddlewareTests
    {
        private ExceptionLoggingMiddleware _middleware;
        private Mock<ITelemetryChannel> _mockTelemetryChannel;
        private TelemetryClient _telemetryClient;

        [SetUp]
        public void Setup()
        {
            _mockTelemetryChannel = new Mock<ITelemetryChannel>();
            _telemetryClient = new TelemetryClient(new TelemetryConfiguration { TelemetryChannel = _mockTelemetryChannel.Object });
            _middleware = new ExceptionLoggingMiddleware(
                next: _ => throw new Exception("Test exception"),
                telemetryClient: _telemetryClient
            );
        }

        [Test]
        public void InvokeAsync_ShouldLogException_WhenExceptionOccurs()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act & Assert
            FluentActions.Invoking(() => _middleware.InvokeAsync(context))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Test exception");

            _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<ExceptionTelemetry>(et =>
                et.Exception.Message == "Test exception" &&
                et.Properties["EceParent"] == "ExceptionRaised" &&
                et.Metrics["EceException"] == 1.0
            )), Times.Once);
        }
    }
}