using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly TelemetryClient _telemetryClient;

    public TestController(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    [HttpGet("throw")]
    public IActionResult ThrowException()
    {
        try
        {
            throw new InvalidOperationException("Test exception for Application Insights.");
        }
        catch (Exception ex)
        {
            _telemetryClient.TrackException(ex, new Dictionary<string, string>
            {
                { "ActionId", "test-action-id" },
                { "ActionName", "ThrowException" },
                { "userEmail", "testuser@example.com" },
                { "UserId", "user-123" }
            });
            return StatusCode(500, "Exception triggered.");
        }
    }
}
