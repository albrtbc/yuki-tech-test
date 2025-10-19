using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Yuki.Blog.Sdk.Configuration;
using Yuki.Blog.Sdk.Exceptions;
using Yuki.Blog.Sdk.Interfaces;
using Yuki.Blog.Sdk.Models.Requests;
using Yuki.Blog.Sdk.Models.Responses;

namespace Yuki.Blog.Sdk;

/// <summary>
/// HTTP client implementation for the Blog API.
/// </summary>
public class BlogClient : IBlogClient
{
    private readonly HttpClient _httpClient;
    private readonly BlogClientOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlogClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API requests.</param>
    /// <param name="options">Configuration options for the client.</param>
    public BlogClient(HttpClient httpClient, IOptions<BlogClientOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Set API version header
        if (!string.IsNullOrEmpty(_options.ApiVersion))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Version", _options.ApiVersion);
        }
    }

    /// <inheritdoc/>
    public async Task<CreatePostResponse> CreatePostAsync(
        CreatePostRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/posts",
                request,
                _jsonOptions,
                cancellationToken);

            await EnsureSuccessStatusCodeAsync(response);

            var result = await response.Content.ReadFromJsonAsync<CreatePostResponse>(
                _jsonOptions,
                cancellationToken);

            return result ?? throw new BlogApiException("Failed to deserialize response");
        }
        catch (BlogApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BlogApiException("An error occurred while creating the post", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<GetPostResponse> GetPostByIdAsync(
        Guid id,
        bool includeAuthor = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/api/posts/{id}";
            if (includeAuthor)
            {
                url += "?include=author";
            }

            var response = await _httpClient.GetAsync(url, cancellationToken);

            await EnsureSuccessStatusCodeAsync(response);

            var result = await response.Content.ReadFromJsonAsync<GetPostResponse>(
                _jsonOptions,
                cancellationToken);

            return result ?? throw new BlogApiException("Failed to deserialize response");
        }
        catch (BlogApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BlogApiException(
                $"An error occurred while retrieving post with ID '{id}'",
                ex);
        }
    }

    /// <summary>
    /// Ensures the HTTP response indicates success, otherwise throws an appropriate exception.
    /// </summary>
    private async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();

        switch (response.StatusCode)
        {
            case HttpStatusCode.NotFound:
                throw new NotFoundException("The requested resource was not found");

            case HttpStatusCode.BadRequest:
                throw new BadRequestException("The request was invalid", content);

            case HttpStatusCode.TooManyRequests:
                TimeSpan? retryAfter = null;
                if (response.Headers.RetryAfter?.Delta.HasValue == true)
                {
                    retryAfter = response.Headers.RetryAfter.Delta.Value;
                }
                throw new RateLimitException(
                    "Rate limit exceeded. Please try again later.",
                    retryAfter);

            case HttpStatusCode.Unauthorized:
                throw new BlogApiException(
                    "Authentication failed. Check your API key.",
                    response.StatusCode,
                    content);

            case HttpStatusCode.Forbidden:
                throw new BlogApiException(
                    "Access forbidden. You don't have permission to access this resource.",
                    response.StatusCode,
                    content);

            case HttpStatusCode.InternalServerError:
                throw new BlogApiException(
                    "An internal server error occurred",
                    response.StatusCode,
                    content);

            default:
                throw new BlogApiException(
                    $"API request failed with status code {(int)response.StatusCode}",
                    response.StatusCode,
                    content);
        }
    }
}
