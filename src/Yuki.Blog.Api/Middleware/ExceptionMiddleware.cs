using System.Net;
using System.Text.Json;

namespace Yuki.Blog.Api.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches UNEXPECTED exceptions (not domain validation) and returns standardized error responses.
/// Domain validation errors are handled through the Result pattern and should not reach this middleware.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Get correlation ID from context (set by CorrelationIdMiddleware)
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier;

        // Log the unexpected exception with rich context
        // Note: CorrelationId is automatically added to logs via LogContext
        _logger.LogError(
            exception,
            "UNEXPECTED EXCEPTION occurred. Path: {Path}, Method: {Method}, User: {User}",
            context.Request.Path,
            context.Request.Method,
            context.User?.Identity?.Name ?? "Anonymous");

        context.Response.ContentType = "application/json";

        // Determine status code based on exception type
        var (statusCode, errorCode) = DetermineErrorDetails(exception);
        context.Response.StatusCode = statusCode;

        var problemDetails = new
        {
            type = GetProblemDetailsType(statusCode),
            title = GetErrorTitle(statusCode),
            status = statusCode,
            correlationId = correlationId,
            traceId = context.TraceIdentifier,
            errorCode = errorCode,
            detail = _environment.IsDevelopment()
                ? exception.Message
                : "An internal server error occurred. Please contact support if the problem persists.",
            stackTrace = _environment.IsDevelopment() ? exception.StackTrace : null,
            innerException = _environment.IsDevelopment() && exception.InnerException != null
                ? exception.InnerException.Message
                : null
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await context.Response.WriteAsync(json);
    }

    private (int statusCode, string errorCode) DetermineErrorDetails(Exception exception)
    {
        return exception switch
        {
            // Infrastructure exceptions (database, network, etc.)
            TimeoutException => (StatusCodes.Status504GatewayTimeout, "Error.Timeout"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Error.Forbidden"),

            // Default to 500 for all other unexpected exceptions
            _ => (StatusCodes.Status500InternalServerError, "Error.Internal")
        };
    }

    private string GetProblemDetailsType(int statusCode)
    {
        return statusCode switch
        {
            500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            504 => "https://tools.ietf.org/html/rfc7231#section-6.6.5",
            403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
    }

    private string GetErrorTitle(int statusCode)
    {
        return statusCode switch
        {
            500 => "Internal Server Error",
            504 => "Gateway Timeout",
            403 => "Forbidden",
            _ => "An error occurred"
        };
    }
}
