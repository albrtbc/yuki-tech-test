using Yuki.Blog.Sdk.Models.Requests;
using Yuki.Blog.Sdk.Models.Responses;

namespace Yuki.Blog.Sdk.Interfaces;

/// <summary>
/// Client interface for interacting with the Blog API.
/// </summary>
public interface IBlogClient
{
    /// <summary>
    /// Creates a new blog post.
    /// </summary>
    /// <param name="request">The post creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created post information.</returns>
    /// <exception cref="Exceptions.BadRequestException">Thrown when the request is invalid.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the author does not exist.</exception>
    /// <exception cref="Exceptions.RateLimitException">Thrown when rate limit is exceeded.</exception>
    /// <exception cref="Exceptions.BlogApiException">Thrown when an API error occurs.</exception>
    Task<CreatePostResponse> CreatePostAsync(
        CreatePostRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a blog post by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the post.</param>
    /// <param name="includeAuthor">Whether to include author information in the response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested post information.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the post is not found.</exception>
    /// <exception cref="Exceptions.BlogApiException">Thrown when an API error occurs.</exception>
    Task<GetPostResponse> GetPostByIdAsync(
        Guid id,
        bool includeAuthor = false,
        CancellationToken cancellationToken = default);
}
