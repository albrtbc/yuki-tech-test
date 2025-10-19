using System.Net;
using FluentAssertions;
using Yuki.Blog.Sdk.Exceptions;
using Yuki.Blog.Sdk.Models.Requests;
using Yuki.Blog.Sdk.UnitTests.Helpers;

namespace Yuki.Blog.Sdk.UnitTests;

/// <summary>
/// Tests for BlogClient error handling scenarios.
/// </summary>
public class BlogClientErrorHandlingTests : IDisposable
{
    private readonly BlogClientTestFixture _fixture;

    public BlogClientErrorHandlingTests()
    {
        _fixture = new BlogClientTestFixture();
    }

    [Fact]
    public async Task CreatePostAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Content = "Test"
        };

        _fixture.SetupErrorResponse(
            HttpMethod.Post,
            "https://api.test.com/api/posts",
            HttpStatusCode.NotFound,
            "Author not found"
        );

        var client = _fixture.CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            async () => await client.CreatePostAsync(request)
        );

        exception.Message.Should().Be("The requested resource was not found");
        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreatePostAsync_WhenBadRequest_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = "", // Invalid: empty title
            Description = "Test",
            Content = "Test"
        };

        var errorContent = "{\"errors\":{\"Title\":[\"Title is required\"]}}";
        _fixture.SetupErrorResponse(
            HttpMethod.Post,
            "https://api.test.com/api/posts",
            HttpStatusCode.BadRequest,
            errorContent
        );

        var client = _fixture.CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            async () => await client.CreatePostAsync(request)
        );

        exception.Message.Should().Be("The request was invalid");
        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.ResponseContent.Should().Be(errorContent);
    }

    [Fact]
    public async Task CreatePostAsync_WhenRateLimited_ShouldThrowRateLimitException()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Content = "Test"
        };

        var retryAfter = TimeSpan.FromSeconds(60);
        _fixture.SetupRateLimitResponse(
            HttpMethod.Post,
            "https://api.test.com/api/posts",
            retryAfter
        );

        var client = _fixture.CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RateLimitException>(
            async () => await client.CreatePostAsync(request)
        );

        exception.Message.Should().Be("Rate limit exceeded. Please try again later.");
        exception.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        exception.RetryAfter.Should().Be(retryAfter);
    }

    [Fact]
    public async Task CreatePostAsync_WhenUnauthorized_ShouldThrowBlogApiException()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Content = "Test"
        };

        _fixture.SetupErrorResponse(
            HttpMethod.Post,
            "https://api.test.com/api/posts",
            HttpStatusCode.Unauthorized,
            "Invalid API key"
        );

        var client = _fixture.CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BlogApiException>(
            async () => await client.CreatePostAsync(request)
        );

        exception.Message.Should().Be("Authentication failed. Check your API key.");
        exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePostAsync_WhenForbidden_ShouldThrowBlogApiException()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Content = "Test"
        };

        _fixture.SetupErrorResponse(
            HttpMethod.Post,
            "https://api.test.com/api/posts",
            HttpStatusCode.Forbidden,
            "Insufficient permissions"
        );

        var client = _fixture.CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BlogApiException>(
            async () => await client.CreatePostAsync(request)
        );

        exception.Message.Should().Be("Access forbidden. You don't have permission to access this resource.");
        exception.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePostAsync_WhenInternalServerError_ShouldThrowBlogApiException()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Content = "Test"
        };

        _fixture.SetupErrorResponse(
            HttpMethod.Post,
            "https://api.test.com/api/posts",
            HttpStatusCode.InternalServerError,
            "Database connection failed"
        );

        var client = _fixture.CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BlogApiException>(
            async () => await client.CreatePostAsync(request)
        );

        exception.Message.Should().Be("An internal server error occurred");
        exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.ResponseContent.Should().Be("Database connection failed");
    }

    [Fact]
    public async Task GetPostByIdAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var postId = Guid.NewGuid();

        _fixture.SetupErrorResponse(
            HttpMethod.Get,
            $"https://api.test.com/api/posts/{postId}",
            HttpStatusCode.NotFound,
            "Post not found"
        );

        var client = _fixture.CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            async () => await client.GetPostByIdAsync(postId)
        );

        exception.Message.Should().Be("The requested resource was not found");
        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPostByIdAsync_WhenRateLimited_ShouldThrowRateLimitExceptionWithoutRetryAfter()
    {
        // Arrange
        var postId = Guid.NewGuid();

        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        // No RetryAfter header set

        _fixture.SetupCustomResponse(HttpMethod.Get, $"https://api.test.com/api/posts/{postId}", response);

        var client = _fixture.CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RateLimitException>(
            async () => await client.GetPostByIdAsync(postId)
        );

        exception.RetryAfter.Should().BeNull();
    }

    [Fact]
    public async Task CreatePostAsync_WhenUnknownStatusCode_ShouldThrowBlogApiException()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Content = "Test"
        };

        _fixture.SetupErrorResponse(
            HttpMethod.Post,
            "https://api.test.com/api/posts",
            HttpStatusCode.BadGateway, // 502
            "Gateway error"
        );

        var client = _fixture.CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BlogApiException>(
            async () => await client.CreatePostAsync(request)
        );

        exception.Message.Should().Be("API request failed with status code 502");
        exception.StatusCode.Should().Be(HttpStatusCode.BadGateway);
    }

    [Fact]
    public async Task GetPostByIdAsync_WhenNetworkException_ShouldThrowBlogApiException()
    {
        // Arrange
        var postId = Guid.NewGuid();

        _fixture.SetupThrowException(HttpMethod.Get, $"https://api.test.com/api/posts/{postId}", new HttpRequestException("Network error"));

        var client = _fixture.CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BlogApiException>(
            async () => await client.GetPostByIdAsync(postId)
        );

        exception.Message.Should().Contain("An error occurred while retrieving post");
        exception.InnerException.Should().BeOfType<HttpRequestException>();
    }

    [Fact]
    public async Task CreatePostAsync_WhenGenericException_ShouldThrowBlogApiException()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Content = "Test"
        };

        _fixture.SetupThrowException(HttpMethod.Post, "https://api.test.com/api/posts", new InvalidOperationException("Unexpected error"));

        var client = _fixture.CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BlogApiException>(
            async () => await client.CreatePostAsync(request)
        );

        exception.Message.Should().Be("An error occurred while creating the post");
        exception.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    public void Dispose()
    {
        _fixture?.Dispose();
        GC.SuppressFinalize(this);
    }
}
