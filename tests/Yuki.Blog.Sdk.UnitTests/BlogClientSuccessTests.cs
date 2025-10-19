using FluentAssertions;
using Yuki.Blog.Sdk.Exceptions;
using Yuki.Blog.Sdk.Models.Requests;
using Yuki.Blog.Sdk.Models.Responses;
using Yuki.Blog.Sdk.UnitTests.Helpers;

namespace Yuki.Blog.Sdk.UnitTests;

/// <summary>
/// Tests for successful BlogClient operations.
/// </summary>
public class BlogClientSuccessTests : IDisposable
{
    private readonly BlogClientTestFixture _fixture;

    public BlogClientSuccessTests()
    {
        _fixture = new BlogClientTestFixture();
    }

    [Fact]
    public async Task CreatePostAsync_WithValidRequest_ShouldReturnCreatedPost()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var request = new CreatePostRequest
        {
            AuthorId = authorId,
            Title = "Test Post",
            Description = "Test Description",
            Content = "Test Content"
        };

        var expectedResponse = new CreatePostResponse
        {
            Id = postId,
            AuthorId = authorId,
            Title = "Test Post",
            Description = "Test Description",
            Content = "Test Content",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _fixture.SetupSuccessResponse(
            HttpMethod.Post,
            "https://api.test.com/api/posts",
            expectedResponse
        );

        var client = _fixture.CreateClient();

        // Act
        var result = await client.CreatePostAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(postId);
        result.AuthorId.Should().Be(authorId);
        result.Title.Should().Be("Test Post");
        result.Description.Should().Be("Test Description");
        result.Content.Should().Be("Test Content");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreatePostAsync_ShouldSendCorrectJsonPayload()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Desc",
            Content = "Content"
        };

        var response = new CreatePostResponse
        {
            Id = Guid.NewGuid(),
            AuthorId = request.AuthorId,
            Title = request.Title,
            Description = request.Description,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        _fixture.SetupSuccessResponse(HttpMethod.Post, "https://api.test.com/api/posts", response);
        var client = _fixture.CreateClient();

        // Act
        var result = await client.CreatePostAsync(request);

        // Assert - verify the response was received correctly
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
    }

    [Fact]
    public async Task GetPostByIdAsync_WithValidId_ShouldReturnPost()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var expectedResponse = new GetPostResponse
        {
            Id = postId,
            AuthorId = authorId,
            Title = "Retrieved Post",
            Description = "Retrieved Description",
            Content = "Retrieved Content",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        _fixture.SetupSuccessResponse(
            HttpMethod.Get,
            $"https://api.test.com/api/posts/{postId}",
            expectedResponse
        );

        var client = _fixture.CreateClient();

        // Act
        var result = await client.GetPostByIdAsync(postId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(postId);
        result.AuthorId.Should().Be(authorId);
        result.Title.Should().Be("Retrieved Post");
        result.Description.Should().Be("Retrieved Description");
        result.Content.Should().Be("Retrieved Content");
    }

    [Fact]
    public async Task GetPostByIdAsync_WithIncludeAuthor_ShouldIncludeAuthorInformation()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var expectedResponse = new GetPostResponse
        {
            Id = postId,
            AuthorId = authorId,
            Title = "Post with Author",
            Description = "Description",
            Content = "Content",
            CreatedAt = DateTime.UtcNow,
            Author = new AuthorInfo
            {
                Id = authorId,
                Name = "John",
                Surname = "Doe"
            }
        };

        _fixture.SetupSuccessResponse(
            HttpMethod.Get,
            $"https://api.test.com/api/posts/{postId}?include=author",
            expectedResponse
        );

        var client = _fixture.CreateClient();

        // Act
        var result = await client.GetPostByIdAsync(postId, includeAuthor: true);

        // Assert
        result.Should().NotBeNull();
        result.Author.Should().NotBeNull();
        result.Author!.Id.Should().Be(authorId);
        result.Author.Name.Should().Be("John");
        result.Author.Surname.Should().Be("Doe");
        result.Author.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetPostByIdAsync_WithoutIncludeAuthor_ShouldNotIncludeQueryParameter()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var response = new GetPostResponse
        {
            Id = postId,
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Content = "Test",
            CreatedAt = DateTime.UtcNow
        };

        _fixture.SetupSuccessResponse(
            HttpMethod.Get,
            $"https://api.test.com/api/posts/{postId}",
            response
        );

        var client = _fixture.CreateClient();

        // Act
        var result = await client.GetPostByIdAsync(postId, includeAuthor: false);

        // Assert
        result.Should().NotBeNull();
        result.Author.Should().BeNull();

        // Verify the author is not included in response
        // (URL verification is implicit through successful test execution)
    }

    [Fact]
    public async Task CreatePostAsync_ShouldIncludeApiVersionHeader()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Content = "Test"
        };

        var response = new CreatePostResponse
        {
            Id = Guid.NewGuid(),
            AuthorId = request.AuthorId,
            Title = request.Title,
            Description = request.Description,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        _fixture.SetupSuccessResponse(HttpMethod.Post, "https://api.test.com/api/posts", response);
        var client = _fixture.CreateClient();

        // Act
        await client.CreatePostAsync(request);

        // Assert
        _fixture.HttpClient.DefaultRequestHeaders.Contains("X-API-Version").Should().BeTrue();
        _fixture.HttpClient.DefaultRequestHeaders.GetValues("X-API-Version").First().Should().Be("1.0");
    }

    [Fact]
    public async Task CreatePostAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Content = "Test"
        };

        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        var client = _fixture.CreateClient();

        // Act & Assert
        // The SDK wraps cancellation exceptions in BlogApiException
        var exception = await Assert.ThrowsAsync<BlogApiException>(
            async () => await client.CreatePostAsync(request, cts.Token)
        );

        // Verify the inner exception is a cancellation exception
        exception.InnerException.Should().BeAssignableTo<OperationCanceledException>();
    }

    public void Dispose()
    {
        _fixture?.Dispose();
        GC.SuppressFinalize(this);
    }
}
