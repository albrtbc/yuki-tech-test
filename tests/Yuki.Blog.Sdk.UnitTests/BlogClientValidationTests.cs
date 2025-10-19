using FluentAssertions;
using Microsoft.Extensions.Options;
using Yuki.Blog.Sdk;
using Yuki.Blog.Sdk.Configuration;
using Yuki.Blog.Sdk.Exceptions;
using Yuki.Blog.Sdk.Models.Requests;
using Yuki.Blog.Sdk.Models.Responses;
using Yuki.Blog.Sdk.UnitTests.Helpers;

namespace Yuki.Blog.Sdk.UnitTests;

/// <summary>
/// Tests for BlogClient validation and edge cases.
/// </summary>
public class BlogClientValidationTests : IDisposable
{
    private readonly BlogClientTestFixture _fixture;

    public BlogClientValidationTests()
    {
        _fixture = new BlogClientTestFixture();
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
    {
        // Arrange
        HttpClient? httpClient = null;
        var options = Options.Create(new BlogClientOptions());

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new BlogClient(httpClient!, options)
        );

        exception.ParamName.Should().Be("httpClient");
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        var httpClient = new HttpClient();
        IOptions<BlogClientOptions>? options = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new BlogClient(httpClient, options!)
        );

        exception.ParamName.Should().Be("options");
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldSetApiVersionHeader()
    {
        // Arrange
        var httpClient = new HttpClient();
        var options = Options.Create(new BlogClientOptions
        {
            ApiVersion = "2.0"
        });

        // Act
        var client = new BlogClient(httpClient, options);

        // Assert
        httpClient.DefaultRequestHeaders.Contains("X-API-Version").Should().BeTrue();
        httpClient.DefaultRequestHeaders.GetValues("X-API-Version").First().Should().Be("2.0");
    }

    [Fact]
    public void Constructor_WithEmptyApiVersion_ShouldNotSetHeader()
    {
        // Arrange
        var httpClient = new HttpClient();
        var options = Options.Create(new BlogClientOptions
        {
            ApiVersion = string.Empty
        });

        // Act
        var client = new BlogClient(httpClient, options);

        // Assert
        httpClient.DefaultRequestHeaders.Contains("X-API-Version").Should().BeFalse();
    }

    [Fact]
    public async Task CreatePostAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        CreatePostRequest? request = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await client.CreatePostAsync(request!)
        );

        exception.ParamName.Should().Be("request");
    }

    [Fact]
    public async Task CreatePostAsync_WithEmptyGuidAuthorId_ShouldStillSendRequest()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.Empty, // Edge case: empty GUID
            Title = "Test",
            Description = "Test",
            Content = "Test"
        };

        var response = new CreatePostResponse
        {
            Id = Guid.NewGuid(),
            AuthorId = Guid.Empty,
            Title = "Test",
            Description = "Test",
            Content = "Test",
            CreatedAt = DateTime.UtcNow
        };

        _fixture.SetupSuccessResponse(HttpMethod.Post, "https://api.test.com/api/posts", response);
        var client = _fixture.CreateClient();

        // Act
        var result = await client.CreatePostAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AuthorId.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task CreatePostAsync_WithEmptyStrings_ShouldStillSendRequest()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = string.Empty,
            Description = string.Empty,
            Content = string.Empty
        };

        var response = new CreatePostResponse
        {
            Id = Guid.NewGuid(),
            AuthorId = request.AuthorId,
            Title = string.Empty,
            Description = string.Empty,
            Content = string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _fixture.SetupSuccessResponse(HttpMethod.Post, "https://api.test.com/api/posts", response);
        var client = _fixture.CreateClient();

        // Act
        var result = await client.CreatePostAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePostAsync_WithVeryLongContent_ShouldHandleCorrectly()
    {
        // Arrange
        var longContent = new string('X', 50000); // Max allowed length
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = "Long Content Post",
            Description = "A post with very long content",
            Content = longContent
        };

        var response = new CreatePostResponse
        {
            Id = Guid.NewGuid(),
            AuthorId = request.AuthorId,
            Title = request.Title,
            Description = request.Description,
            Content = longContent,
            CreatedAt = DateTime.UtcNow
        };

        _fixture.SetupSuccessResponse(HttpMethod.Post, "https://api.test.com/api/posts", response);
        var client = _fixture.CreateClient();

        // Act
        var result = await client.CreatePostAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Content.Length.Should().Be(50000);
    }

    [Fact]
    public async Task GetPostByIdAsync_WithEmptyGuid_ShouldStillMakeRequest()
    {
        // Arrange
        var postId = Guid.Empty;
        var response = new GetPostResponse
        {
            Id = Guid.Empty,
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Content = "Test",
            CreatedAt = DateTime.UtcNow
        };

        _fixture.SetupSuccessResponse(
            HttpMethod.Get,
            $"https://api.test.com/api/posts/{Guid.Empty}",
            response
        );

        var client = _fixture.CreateClient();

        // Act
        var result = await client.GetPostByIdAsync(postId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task CreatePostAsync_WhenResponseIsNull_ShouldThrowBlogApiException()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Content = "Test"
        };

        // Setup a response that returns null JSON (edge case)
        _fixture.SetupContentResponse(HttpMethod.Post, "https://api.test.com/api/posts", "application/json", "null");

        var client = _fixture.CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BlogApiException>(
            async () => await client.CreatePostAsync(request)
        );

        exception.Message.Should().Be("Failed to deserialize response");
    }

    [Fact]
    public async Task GetPostByIdAsync_WhenResponseIsNull_ShouldThrowBlogApiException()
    {
        // Arrange
        var postId = Guid.NewGuid();

        _fixture.SetupContentResponse(HttpMethod.Get, $"https://api.test.com/api/posts/{postId}", "application/json", "null");

        var client = _fixture.CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BlogApiException>(
            async () => await client.GetPostByIdAsync(postId)
        );

        exception.Message.Should().Be("Failed to deserialize response");
    }

    [Fact]
    public async Task CreatePostAsync_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test <>&\"' Special Chars",
            Description = "Unicode: 你好 🎉 émoji",
            Content = "Content with\nnewlines\tand\ttabs"
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

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test <>&\"' Special Chars");
        result.Description.Should().Contain("你好");
        result.Content.Should().Contain("\n");
    }

    [Fact]
    public async Task GetPostByIdAsync_WithBothIncludeAuthorValues_ShouldBuildCorrectUrls()
    {
        // Test both true and false to ensure URL is built correctly
        var postId = Guid.NewGuid();
        var responseWithAuthor = new GetPostResponse
        {
            Id = postId,
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Content = "Test",
            CreatedAt = DateTime.UtcNow,
            Author = new AuthorInfo { Id = Guid.NewGuid(), Name = "John", Surname = "Doe" }
        };

        var responseWithoutAuthor = new GetPostResponse
        {
            Id = postId,
            AuthorId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Content = "Test",
            CreatedAt = DateTime.UtcNow
        };

        // Test includeAuthor = true
        _fixture.SetupSuccessResponse(
            HttpMethod.Get,
            $"https://api.test.com/api/posts/{postId}?include=author",
            responseWithAuthor
        );

        var client1 = _fixture.CreateClient();
        var resultWithAuthor = await client1.GetPostByIdAsync(postId, includeAuthor: true);
        resultWithAuthor.Author.Should().NotBeNull();

        // Create new fixture for second test to avoid conflicts
        using var fixture2 = new BlogClientTestFixture();
        fixture2.SetupSuccessResponse(
            HttpMethod.Get,
            $"https://api.test.com/api/posts/{postId}",
            responseWithoutAuthor
        );

        var client2 = fixture2.CreateClient();
        var resultWithoutAuthor = await client2.GetPostByIdAsync(postId, includeAuthor: false);
        resultWithoutAuthor.Author.Should().BeNull();
    }

    [Fact]
    public void AuthorInfo_FullName_ShouldCombineNameAndSurname()
    {
        // Arrange
        var author = new AuthorInfo
        {
            Id = Guid.NewGuid(),
            Name = "Jane",
            Surname = "Smith"
        };

        // Act & Assert
        author.FullName.Should().Be("Jane Smith");
    }

    [Fact]
    public void AuthorInfo_FullName_WithEmptyName_ShouldTrim()
    {
        // Arrange
        var author = new AuthorInfo
        {
            Id = Guid.NewGuid(),
            Name = string.Empty,
            Surname = "Smith"
        };

        // Act & Assert
        author.FullName.Should().Be("Smith");
    }

    [Fact]
    public void AuthorInfo_FullName_WithEmptySurname_ShouldTrim()
    {
        // Arrange
        var author = new AuthorInfo
        {
            Id = Guid.NewGuid(),
            Name = "Jane",
            Surname = string.Empty
        };

        // Act & Assert
        author.FullName.Should().Be("Jane");
    }

    public void Dispose()
    {
        _fixture?.Dispose();
        GC.SuppressFinalize(this);
    }
}
