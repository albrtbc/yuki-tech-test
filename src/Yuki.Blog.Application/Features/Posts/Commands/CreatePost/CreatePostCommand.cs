using MediatR;
using Yuki.Blog.Application.Common.Models;

namespace Yuki.Blog.Application.Features.Posts.Commands.CreatePost;

/// <summary>
/// Command to create a new blog post.
/// </summary>
public record CreatePostCommand : IRequest<ApplicationResult<CreatePostResponse>>
{
    /// <summary>
    /// The ID of the author creating the post.
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
}
