namespace Yuki.Blog.Sdk.Models.Requests;

/// <summary>
/// Request model for creating a new blog post.
/// </summary>
public record CreatePostRequest
{
    /// <summary>
    /// The ID of the author creating the post.
    /// </summary>
    public Guid AuthorId { get; init; }

    /// <summary>
    /// The title of the post. Maximum 200 characters.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// A brief description of the post. Maximum 500 characters.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The full content of the post. Maximum 50,000 characters.
    /// </summary>
    public string Content { get; init; } = string.Empty;
}
