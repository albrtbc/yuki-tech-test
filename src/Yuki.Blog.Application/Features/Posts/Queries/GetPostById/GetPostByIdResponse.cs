namespace Yuki.Blog.Application.Features.Posts.Queries.GetPostById;

/// <summary>
/// Response model for post queries in the Application layer.
/// </summary>
public record GetPostByIdResponse
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

    /// <summary>
    /// Author information (only populated if requested).
    /// </summary>
    public GetPostByIdAuthorResponse? Author { get; init; }
}
