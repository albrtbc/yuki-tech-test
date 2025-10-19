using MediatR;
using Microsoft.Extensions.Logging;
using Yuki.Blog.Domain.Events;

namespace Yuki.Blog.Application.Features.Posts.EventHandlers;

/// <summary>
/// Creates an audit trail record when a post is created.
/// In a real application, this would persist to an audit log table or external audit system.
/// For demonstration purposes, this logs the audit entry.
/// </summary>
public class AuditPostCreatedHandler : INotificationHandler<PostCreatedEvent>
{
    private readonly ILogger<AuditPostCreatedHandler> _logger;

    public AuditPostCreatedHandler(ILogger<AuditPostCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PostCreatedEvent notification, CancellationToken cancellationToken)
    {
        // In a real application, you would:
        // 1. Create an audit log entry
        // 2. Include metadata (user, IP address, timestamp, etc.)
        // 3. Store in an audit table or send to an audit service

        _logger.LogInformation(
            "Domain Event: Audit trail - Action: PostCreated, PostId: {PostId}, Title: {Title}, AuthorId: {AuthorId}, Timestamp: {Timestamp}",
            notification.PostId,
            notification.Title,
            notification.AuthorId,
            notification.OccurredOn);

        return Task.CompletedTask;
    }
}
