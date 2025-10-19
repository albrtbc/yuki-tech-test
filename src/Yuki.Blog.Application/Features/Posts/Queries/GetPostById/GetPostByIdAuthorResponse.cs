namespace Yuki.Blog.Application.Features.Posts.Queries.GetPostById;

/// <summary>
/// Response model for author information in the Application layer.
/// </summary>
public record GetPostByIdAuthorResponse
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
}
