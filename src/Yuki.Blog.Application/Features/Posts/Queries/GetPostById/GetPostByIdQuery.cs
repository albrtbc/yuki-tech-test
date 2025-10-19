using MediatR;
using Yuki.Blog.Application.Common.Models;

namespace Yuki.Blog.Application.Features.Posts.Queries.GetPostById;

/// <summary>
/// Query to retrieve a post by its unique identifier.
/// </summary>
public record GetPostByIdQuery : IRequest<ApplicationResult<GetPostByIdResponse>>
{
    /// <summary>
    /// The unique identifier of the post to retrieve.
    /// </summary>
    public Guid PostId { get; init; }

    /// <summary>
    /// The list of related resources to include in the response.
    /// Supported values: "author"
    /// </summary>
    public IReadOnlyList<string> Includes { get; init; }

    /// <summary>
    /// Whether to include author information in the response.
    /// </summary>
    public bool IncludeAuthor => Includes.Any(i =>
        i.Equals("author", StringComparison.OrdinalIgnoreCase));

    public GetPostByIdQuery(Guid postId, IReadOnlyList<string>? includes = null)
    {
        PostId = postId;
        Includes = includes ?? Array.Empty<string>();
    }
}
