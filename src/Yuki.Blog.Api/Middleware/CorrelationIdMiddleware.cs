using System.Diagnostics;
using Serilog;
using Serilog.Context;

namespace Yuki.Blog.Api.Middleware;

/// <summary>
/// Middleware that ensures every request has a correlation ID for distributed tracing.
/// Uses OpenTelemetry's TraceId as the correlation ID to provide a single source of truth
/// for tracking requests across logs, traces, and services.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string CorrelationIdItemKey = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private readonly IDiagnosticContext _diagnosticContext;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger,
        IDiagnosticContext diagnosticContext)
    {
        _next = next;
        _logger = logger;
        _diagnosticContext = diagnosticContext;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if caller provided a correlation ID, otherwise generate one
        // This enables distributed tracing across service boundaries
        var correlationId = GetOrCreateCorrelationId(context);

        // Store in HttpContext.Items for easy access throughout the request pipeline
        context.Items[CorrelationIdItemKey] = correlationId;

        // Add to response headers so clients can correlate their requests with logs and traces
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
            {
                context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);
            }
            return Task.CompletedTask;
        });

        // Enrich the entire request context with correlation ID
        // This ensures it appears in ALL logs for this request, including ASP.NET Core's built-in logs
        _diagnosticContext.Set("CorrelationId", correlationId);

        // Also push to LogContext for backwards compatibility and nested scopes
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogDebug("Request started with TraceId/CorrelationId: {CorrelationId}", correlationId);

            await _next(context);

            _logger.LogDebug("Request completed with TraceId/CorrelationId: {CorrelationId}", correlationId);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        // Priority 1: Check if the caller provided an explicit X-Correlation-ID header
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationIdFromHeader)
            && !string.IsNullOrWhiteSpace(correlationIdFromHeader))
        {
            // Use the caller's correlation ID to maintain tracing across service boundaries
            return correlationIdFromHeader.ToString();
        }

        // Priority 2: Extract TraceId from W3C traceparent header (standard for OpenTelemetry)
        // Format: "00-{traceId}-{spanId}-{flags}" where traceId is 32 hex chars
        if (context.Request.Headers.TryGetValue("traceparent", out var traceparent)
            && !string.IsNullOrWhiteSpace(traceparent))
        {
            var traceId = ExtractTraceIdFromTraceparent(traceparent.ToString());
            if (!string.IsNullOrEmpty(traceId))
            {
                return traceId;
            }
        }

        // Priority 3: Use OpenTelemetry's TraceId from current Activity
        // Activity.Current is automatically created by ASP.NET Core's OpenTelemetry instrumentation
        var activity = Activity.Current;

        if (activity != null && activity.TraceId != default)
        {
            return activity.TraceId.ToString();
        }

        // Fallback: Generate a new GUID if Activity is not available (shouldn't happen with OpenTelemetry configured)
        // Use "N" format to get 32-char hex string without hyphens, matching TraceId format
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Extracts the trace ID from a W3C traceparent header.
    /// </summary>
    /// <param name="traceparent">The traceparent header value (format: "00-{traceId}-{spanId}-{flags}").</param>
    /// <returns>The 32-character trace ID, or null if invalid.</returns>
    private static string? ExtractTraceIdFromTraceparent(string traceparent)
    {
        // W3C Trace Context format: "00-{traceId}-{spanId}-{flags}"
        // Example: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
        var parts = traceparent.Split('-');

        if (parts.Length >= 2 && parts[1].Length == 32)
        {
            // Return the traceId (second part)
            return parts[1];
        }

        return null;
    }
}

/// <summary>
/// Extension methods for registering the CorrelationIdMiddleware.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    /// <summary>
    /// Adds the CorrelationId middleware to the application pipeline.
    /// This should be added early in the pipeline to ensure all subsequent middleware
    /// and request processing can access the correlation ID.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}
