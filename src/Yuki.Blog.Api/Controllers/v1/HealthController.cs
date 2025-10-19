using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Yuki.Blog.Api.Contracts.v1.Responses;

namespace Yuki.Blog.Api.Controllers.v1;

/// <summary>
/// API controller for health checks.
/// </summary>
/// <remarks>
/// This controller provides health check information for monitoring and diagnostics.
/// This API uses header-based versioning. Include the X-API-Version header in your requests.
/// Example: X-API-Version: 1.0
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/health")]
[Produces("application/json", "application/xml")]
[ApiExplorerSettings(GroupName = "v1")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        HealthCheckService healthCheckService,
        ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the health status of the application and its dependencies.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Health status information.</returns>
    /// <response code="200">Returns healthy status.</response>
    /// <response code="503">Returns unhealthy or degraded status.</response>
    /// <remarks>
    /// **API Versioning:**
    /// This endpoint uses header-based versioning. Include the X-API-Version header.
    /// Example: X-API-Version: 1.0
    ///
    /// **Content Negotiation:**
    /// Supports both JSON (default) and XML response formats.
    /// - JSON: Accept: application/json
    /// - XML: Accept: application/xml
    ///
    /// **Health Check Information:**
    /// - Checks database connectivity
    /// - Reports overall application health status
    /// - Provides details on individual health check results
    ///
    /// **Possible statuses:**
    /// - Healthy: All checks passed
    /// - Degraded: Some non-critical checks failed
    /// - Unhealthy: Critical checks failed
    ///
    /// **Response Caching:**
    /// This endpoint is cached for 10 seconds to reduce database load from frequent health check probes
    /// while still providing near-real-time health status information.
    /// </remarks>
    [HttpGet]
    [ResponseCache(Duration = 10)]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Health check endpoint called");

        var healthReport = await _healthCheckService.CheckHealthAsync(cancellationToken);

        var response = new HealthCheckResponse
        {
            Status = healthReport.Status.ToString(),
            TotalDuration = healthReport.TotalDuration.TotalMilliseconds,
            Checks = healthReport.Entries.Select(entry => new HealthCheckEntry
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Duration = entry.Value.Duration.TotalMilliseconds,
                Description = entry.Value.Description,
                Exception = entry.Value.Exception?.Message,
                Data = entry.Value.Data.Any() ? entry.Value.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : null
            }).ToList()
        };

        var statusCode = healthReport.Status == HealthStatus.Healthy
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        return StatusCode(statusCode, response);
    }
}
