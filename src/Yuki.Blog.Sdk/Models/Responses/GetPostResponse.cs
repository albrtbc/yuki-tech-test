namespace Yuki.Blog.Sdk.Models.Responses;

/// <summary>
/// Response model for getting blog post information.
/// </summary>
public record GetPostResponse
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
    /// Author information (only populated if requested with include=author).
    /// </summary>
    public AuthorInfo? Author { get; init; }
}

/// <summary>
/// Author information within a post response.
/// </summary>
public record AuthorInfo
{
    /// <summary>
    /// The unique identifier of the author.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The author's first name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The author's last name.
    /// </summary>
    public string Surname { get; init; } = string.Empty;

    /// <summary>
    /// The author's full name (computed property).
    /// </summary>
    public string FullName => $"{Name} {Surname}".Trim();
}
