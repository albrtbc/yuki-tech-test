using Microsoft.Extensions.Logging;
using Yuki.Blog.Application.Common.Interfaces;
using Yuki.Blog.Domain.Events;

namespace Yuki.Blog.Application.Features.Posts.EventHandlers;

/// <summary>
/// Updates author statistics when a post is created.
/// This is a domain event handler because updating author statistics is a business concern.
/// In a real application, this would update a read model or cache with aggregated statistics.
/// For demonstration purposes, this logs the statistics update.
/// </summary>
public class UpdateAuthorStatsHandler : IDomainEventHandler<PostCreatedEvent>
{
    private readonly ILogger<UpdateAuthorStatsHandler> _logger;

    public UpdateAuthorStatsHandler(ILogger<UpdateAuthorStatsHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PostCreatedEvent notification, CancellationToken cancellationToken)
    {
        // In a real application, you would:
        // 1. Query a read model table (e.g., AuthorStatistics)
        // 2. Increment the post count for the author
        // 3. Update the last post date
        // 4. Save the changes

        _logger.LogInformation(
            "Domain Event: Updating author statistics - AuthorId: {AuthorId}, New post count would be incremented, Last post date: {Date}",
            notification.AuthorId,
            notification.OccurredOn);

        return Task.CompletedTask;
    }
}
