using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Yuki.Blog.Api.Contracts.v1.Mappings;
using Yuki.Blog.Api.Contracts.v1.Requests;
using Yuki.Blog.Api.Contracts.v1.Responses;
using Yuki.Blog.Api.Mapping;
using Yuki.Blog.Application.Features.Posts.Queries.GetPostById;

namespace Yuki.Blog.Api.Controllers.v1;

/// <summary>
/// API controller for managing blog posts.
/// </summary>
/// <remarks>
/// This API uses header-based versioning. Include the X-API-Version header in your requests.
/// Example: X-API-Version: 1.0
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/posts")]
[Produces("application/json", "application/xml")]
public class PostsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger<PostsController> _logger;
    private readonly IResultMapper _resultMapper;

    public PostsController(
        ISender mediator,
        ILogger<PostsController> logger,
        IResultMapper resultMapper)
    {
        _mediator = mediator;
        _logger = logger;
        _resultMapper = resultMapper;
    }

    /// <summary>
    /// Creates a new blog post.
    /// </summary>
    /// <param name="request">The request containing post details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created post.</returns>
    /// <response code="201">Returns the newly created post.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the author does not exist.</response>
    /// <response code="429">Too many requests.</response>
    /// <remarks>
    /// **API Versioning:**
    /// This endpoint uses header-based versioning. Include the X-API-Version header.
    /// Example: X-API-Version: 1.0
    ///
    /// **Rate Limiting:**
    /// This endpoint is rate limited to 10 requests per minute per IP address.
    /// Additional requests will receive a 429 Too Many Requests response.
    /// </remarks>
    [HttpPost]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(Contracts.v1.Responses.CreatePostResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreatePost(
        [FromBody] CreatePostRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new post with title: {Title}", request.Title);

        // Map API request to Application command
        var command = request.ToCommand();
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to create post: {Error}", result.Error.Message);
        }
        else
        {
            _logger.LogInformation("Successfully created post with ID: {PostId}", result.Value.Id);
        }

        // Use result mapper to convert result to action result
        return _resultMapper.ToActionResult(result, value =>
        {
            var apiResponse = value.ToApiResponse();
            return CreatedAtAction(
                nameof(GetPostById),
                new { id = apiResponse.Id },
                apiResponse);
        });
    }

    /// <summary>
    /// Retrieves a blog post by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the post.</param>
    /// <param name="queryParams">Query parameters for including related resources.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested post.</returns>
    /// <response code="200">Returns the requested post.</response>
    /// <response code="404">If the post is not found.</response>
    /// <remarks>
    /// **API Versioning:**
    /// This endpoint uses header-based versioning. Include the X-API-Version header.
    /// Example: X-API-Version: 1.0
    ///
    /// **Example requests:**
    ///
    ///     GET /api/posts/{id}
    ///     GET /api/posts/{id}?include=author
    ///
    /// **Supported include values:**
    /// - author: Include the author information
    /// - tags: Include post tags (future enhancement)
    /// - comments: Include post comments (future enhancement)
    ///
    /// **Response Caching:**
    /// This endpoint is cached for 60 seconds. Responses vary by the "include" query parameter.
    /// Different include values will result in separate cache entries.
    /// </remarks>
    [HttpGet("{id:guid}")]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "include" })]
    [ProducesResponseType(typeof(Contracts.v1.Responses.GetPostResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPostById(
        [FromRoute] Guid id,
        [FromQuery] PostQueryParameters queryParams,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving post with ID: {PostId}, Includes: {Includes}",
            id, string.Join(", ", queryParams.Includes));

        var query = new GetPostByIdQuery(id, queryParams.Includes);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Post not found with ID: {PostId}", id);
        }
        else
        {
            _logger.LogInformation("Successfully retrieved post with ID: {PostId}", id);
        }

        // Use result mapper to convert result to action result
        return _resultMapper.ToActionResult(result, value =>
        {
            var apiResponse = value.ToApiResponse();
            return Ok(apiResponse);
        });
    }
}
