using MediatR;
using Microsoft.Extensions.Logging;
using Yuki.Blog.Domain.Events;

namespace Yuki.Blog.Application.Features.Posts.EventHandlers;

/// <summary>
/// Logs when a new post is created.
/// Demonstrates simple domain event handling for audit and debugging purposes.
/// </summary>
public class LogPostCreatedHandler : INotificationHandler<PostCreatedEvent>
{
    private readonly ILogger<LogPostCreatedHandler> _logger;

    public LogPostCreatedHandler(ILogger<LogPostCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PostCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain Event: Post created - PostId: {PostId}, Title: {Title}, AuthorId: {AuthorId}, OccurredOn: {OccurredOn}",
            notification.PostId,
            notification.Title,
            notification.AuthorId,
            notification.OccurredOn);

        return Task.CompletedTask;
    }
}
