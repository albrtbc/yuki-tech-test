namespace Yuki.Blog.Application.Features.Posts.Commands.CreatePost;

/// <summary>
/// Response model for post creation in the Application layer.
/// This is a simplified response without related entities like Author.
/// </summary>
public record CreatePostResponse
{
    /// <summary>
    /// The unique identifier of the post.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The ID of the author who created the post.
    /// </summary>
    public Guid AuthorId { get; init; }

    /// <summary>
    /// The title of the post.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// A brief description of the post.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The full content of the post.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// When the post was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the post was last updated (null if never updated).
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}
