using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Yuki.Blog.Application.Common.Behaviors;

/// <summary>
/// Custom MediatR notification publisher that wraps each domain event handler execution
/// with OpenTelemetry tracing to create spans for distributed tracing visibility.
/// </summary>
public class TracingNotificationPublisher : INotificationPublisher
{
    private readonly ILogger<TracingNotificationPublisher> _logger;
    private static readonly ActivitySource ActivitySource = new("Yuki.Blog.Application.MediatR");

    public TracingNotificationPublisher(ILogger<TracingNotificationPublisher> logger)
    {
        _logger = logger;
    }

    public async Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
    {
        var notificationName = notification.GetType().Name;

        foreach (var handlerExecutor in handlerExecutors)
        {
            var handlerType = handlerExecutor.HandlerInstance.GetType();
            var handlerName = handlerType.Name;

            // Skip if this is a generic tracing wrapper to avoid recursion
            if (handlerType.IsGenericType && handlerType.GetGenericTypeDefinition().Name.Contains("Tracing"))
            {
                await handlerExecutor.HandlerCallback(notification, cancellationToken);
                continue;
            }

            // Create an OpenTelemetry Activity (span) for each domain event handler
            using var activity = ActivitySource.StartActivity(
                $"DomainEvent.{notificationName}.{handlerName}",
                ActivityKind.Internal);

            // Add tags to the activity for better observability
            activity?.SetTag("domain_event.type", notificationName);
            activity?.SetTag("domain_event.handler", handlerName);
            activity?.SetTag("domain_event.assembly", notification.GetType().Assembly.GetName().Name);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await handlerExecutor.HandlerCallback(notification, cancellationToken);

                stopwatch.Stop();

                // Record success metrics
                activity?.SetTag("domain_event.success", true);
                activity?.SetTag("domain_event.duration_ms", stopwatch.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogDebug(
                    "Domain event handler {HandlerName} processed {NotificationName} in {ElapsedMilliseconds}ms",
                    handlerName,
                    notificationName,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Record failure metrics
                activity?.SetTag("domain_event.success", false);
                activity?.SetTag("domain_event.duration_ms", stopwatch.ElapsedMilliseconds);
                activity?.SetTag("domain_event.error.type", ex.GetType().Name);
                activity?.SetTag("domain_event.error.message", ex.Message);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                _logger.LogError(
                    ex,
                    "Domain event handler {HandlerName} failed processing {NotificationName} after {ElapsedMilliseconds}ms",
                    handlerName,
                    notificationName,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }
}
