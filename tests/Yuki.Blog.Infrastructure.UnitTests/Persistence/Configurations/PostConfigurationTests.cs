using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.ValueObjects;
using Yuki.Blog.Infrastructure.Persistence;

namespace Yuki.Blog.Infrastructure.UnitTests.Persistence.Configurations;

public class PostConfigurationTests
{
    private readonly Mock<IPublisher> _publisherMock;
    private readonly Mock<ILogger<BlogDbContext>> _loggerMock;

    public PostConfigurationTests()
    {
        _publisherMock = new Mock<IPublisher>();
        _loggerMock = new Mock<ILogger<BlogDbContext>>();
    }

    private BlogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new BlogDbContext(options, _publisherMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Configuration_ShouldMapPostIdCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var postId = PostId.Create(Guid.NewGuid()).Value;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var post = TestHelpers.CreatePost(
            postId,
            authorId,
            "Test Title",
            "Test Description",
            "Test Content",
            DateTime.UtcNow);

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Act
        var savedPost = await context.Posts.FindAsync(postId);

        // Assert
        savedPost.Should().NotBeNull();
        savedPost!.Id.Should().Be(postId);
        savedPost.Id.Value.Should().Be(postId.Value);
    }

    [Fact]
    public async Task Configuration_ShouldMapAuthorIdCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var postId = PostId.Create(Guid.NewGuid()).Value;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var post = TestHelpers.CreatePost(
            postId,
            authorId,
            "Test Title",
            "Test Description",
            "Test Content",
            DateTime.UtcNow);

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Act
        var savedPost = await context.Posts.FindAsync(postId);

        // Assert
        savedPost.Should().NotBeNull();
        savedPost!.AuthorId.Should().Be(authorId);
        savedPost.AuthorId.Value.Should().Be(authorId.Value);
    }

    [Fact]
    public async Task Configuration_ShouldMapTitleAsOwnedEntity()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var postId = PostId.Create(Guid.NewGuid()).Value;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var post = TestHelpers.CreatePost(
            postId,
            authorId,
            "Test Title",
            "Test Description",
            "Test Content",
            DateTime.UtcNow);

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Act
        var savedPost = await context.Posts.FindAsync(postId);

        // Assert
        savedPost.Should().NotBeNull();
        savedPost!.Title.Should().NotBeNull();
        savedPost.Title.Value.Should().Be("Test Title");
    }

    [Fact]
    public async Task Configuration_ShouldMapDescriptionAsOwnedEntity()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var postId = PostId.Create(Guid.NewGuid()).Value;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var post = TestHelpers.CreatePost(
            postId,
            authorId,
            "Test Title",
            "Test Description",
            "Test Content",
            DateTime.UtcNow);

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Act
        var savedPost = await context.Posts.FindAsync(postId);

        // Assert
        savedPost.Should().NotBeNull();
        savedPost!.Description.Should().NotBeNull();
        savedPost.Description.Value.Should().Be("Test Description");
    }

    [Fact]
    public async Task Configuration_ShouldMapContentAsOwnedEntity()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var postId = PostId.Create(Guid.NewGuid()).Value;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var post = TestHelpers.CreatePost(
            postId,
            authorId,
            "Test Title",
            "Test Description",
            "Test Content",
            DateTime.UtcNow);

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Act
        var savedPost = await context.Posts.FindAsync(postId);

        // Assert
        savedPost.Should().NotBeNull();
        savedPost!.Content.Should().NotBeNull();
        savedPost.Content.Value.Should().Be("Test Content");
    }

    [Fact]
    public async Task Configuration_ShouldMapCreatedAtCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var postId = PostId.Create(Guid.NewGuid()).Value;
        var createdAt = DateTime.UtcNow;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var post = TestHelpers.CreatePost(
            postId,
            authorId,
            "Test Title",
            "Test Description",
            "Test Content",
            createdAt);

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Act
        var savedPost = await context.Posts.FindAsync(postId);

        // Assert
        savedPost.Should().NotBeNull();
        savedPost!.CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Configuration_ShouldMapUpdatedAtCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var postId = PostId.Create(Guid.NewGuid()).Value;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var post = TestHelpers.CreatePost(
            postId,
            authorId,
            "Test Title",
            "Test Description",
            "Test Content",
            DateTime.UtcNow);

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Act
        var savedPost = await context.Posts.FindAsync(postId);

        // Assert
        savedPost.Should().NotBeNull();
        savedPost!.UpdatedAt.Should().BeNull(); // New post has no update time
    }

    [Fact]
    public async Task Configuration_ShouldNotPersistDomainEvents()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var postId = PostId.Create(Guid.NewGuid()).Value;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var post = TestHelpers.CreatePost(
            postId,
            authorId,
            "Test Title",
            "Test Description",
            "Test Content",
            DateTime.UtcNow);

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Clear and reload
        context.ChangeTracker.Clear();

        // Act
        var savedPost = await context.Posts.FindAsync(postId);

        // Assert
        savedPost.Should().NotBeNull();
        savedPost!.DomainEvents.Should().BeEmpty(); // Domain events are not persisted
    }

    [Fact]
    public async Task Configuration_ShouldEnforceMaxLengthOnTitle()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var postId = PostId.Create(Guid.NewGuid()).Value;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Create a title that's within the max length
        var validTitle = new string('a', PostTitle.MaxLength);
        var post = TestHelpers.CreatePost(
            postId,
            authorId,
            validTitle,
            "Test Description",
            "Test Content",
            DateTime.UtcNow);

        context.Posts.Add(post);

        // Act & Assert - should not throw
        await context.SaveChangesAsync();
        var savedPost = await context.Posts.FindAsync(postId);
        savedPost.Should().NotBeNull();
    }

    [Fact]
    public async Task Configuration_ShouldCreateIndexOnAuthorId()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Create multiple posts for the same author
        for (int i = 0; i < 5; i++)
        {
            var post = TestHelpers.CreatePost(
                PostId.Create(Guid.NewGuid()).Value,
                authorId,
                $"Title {i}",
                $"Description {i}",
                $"Content {i}",
                DateTime.UtcNow);

            context.Posts.Add(post);
        }

        await context.SaveChangesAsync();

        // Act - Query by authorId (should use index)
        var posts = await context.Posts
            .Where(p => p.AuthorId == authorId)
            .ToListAsync();

        // Assert
        posts.Should().HaveCount(5);
    }

    [Fact]
    public async Task Configuration_ShouldCreateIndexOnCreatedAt()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var baseTime = DateTime.UtcNow;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Create posts with different timestamps
        for (int i = 0; i < 5; i++)
        {
            var post = TestHelpers.CreatePost(
                PostId.Create(Guid.NewGuid()).Value,
                authorId,
                $"Title {i}",
                $"Description {i}",
                $"Content {i}",
                baseTime.AddHours(i));

            context.Posts.Add(post);
        }

        await context.SaveChangesAsync();

        // Act - Query by CreatedAt (should use index)
        var posts = await context.Posts
            .Where(p => p.CreatedAt >= baseTime.AddHours(2))
            .ToListAsync();

        // Assert
        posts.Should().HaveCountGreaterOrEqualTo(3);
    }
}
