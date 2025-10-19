using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Yuki.Blog.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs request/response information and execution time.
/// Integrates with OpenTelemetry for distributed tracing and observability.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private static readonly ActivitySource ActivitySource = new("Yuki.Blog.Application.MediatR");

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Create an OpenTelemetry Activity (span) for this MediatR request
        using var activity = ActivitySource.StartActivity(
            $"MediatR.{requestName}",
            ActivityKind.Internal);

        // Add tags to the activity for better observability
        activity?.SetTag("mediatr.request.type", requestName);
        activity?.SetTag("mediatr.request.assembly", typeof(TRequest).Assembly.GetName().Name);

        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Handling {RequestName}",
            requestName);

        TResponse response;
        try
        {
            response = await next();

            stopwatch.Stop();

            // Record success metrics
            activity?.SetTag("mediatr.request.success", true);
            activity?.SetTag("mediatr.request.duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Record failure metrics
            activity?.SetTag("mediatr.request.success", false);
            activity?.SetTag("mediatr.request.duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetTag("mediatr.request.error.type", ex.GetType().Name);
            activity?.SetTag("mediatr.request.error.message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Request {RequestName} failed after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }

        _logger.LogInformation(
            "Handled {RequestName} in {ElapsedMilliseconds}ms",
            requestName,
            stopwatch.ElapsedMilliseconds);

        return response;
    }
}
