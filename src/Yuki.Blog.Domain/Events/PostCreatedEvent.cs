using Yuki.Blog.Domain.Common;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Domain.Events;

/// <summary>
/// Domain event raised when a new post is created.
/// </summary>
public sealed class PostCreatedEvent : IDomainEvent
{
    public PostId PostId { get; }
    public AuthorId AuthorId { get; }
    public string Title { get; }
    public DateTime OccurredOn { get; }

    public PostCreatedEvent(PostId postId, AuthorId authorId, string title, DateTime occurredOn)
    {
        PostId = postId;
        AuthorId = authorId;
        Title = title;
        OccurredOn = occurredOn;
    }
}
