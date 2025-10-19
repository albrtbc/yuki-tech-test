using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.ValueObjects;
using Yuki.Blog.Infrastructure.Persistence;
using Yuki.Blog.Infrastructure.Persistence.ReadOnlyRepositories;

namespace Yuki.Blog.Infrastructure.UnitTests.Persistence.ReadOnlyRepositories;

public class PostReadOnlyRepositoryTests
{
    private readonly Mock<IPublisher> _publisherMock;
    private readonly Mock<ILogger<BlogDbContext>> _loggerMock;

    public PostReadOnlyRepositoryTests()
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
    public async Task GetByIdAsync_WhenPostExists_ShouldReturnPostReadDto()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostReadOnlyRepository(context);

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
        var result = await repository.GetByIdAsync(postId.Value);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(postId.Value);
        result.AuthorId.Should().Be(authorId.Value);
        result.Title.Should().Be("Test Title");
        result.Description.Should().Be("Test Description");
        result.Content.Should().Be("Test Content");
        result.Author.Should().BeNull(); // Author is not included
    }

    [Fact]
    public async Task GetByIdAsync_WhenPostDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostReadOnlyRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldUseAsNoTracking()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostReadOnlyRepository(context);

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

        // Clear the change tracker to simulate a fresh query
        context.ChangeTracker.Clear();

        // Act
        var result = await repository.GetByIdAsync(postId.Value);

        // Assert
        result.Should().NotBeNull();

        // Verify no entities are tracked (AsNoTracking ensures this)
        context.ChangeTracker.Entries<Post>().Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldProjectDirectlyToDto()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostReadOnlyRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var postId = PostId.Create(Guid.NewGuid()).Value;
        var createdAt = DateTime.UtcNow;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", createdAt);
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
        var result = await repository.GetByIdAsync(postId.Value);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(postId.Value);
        result.AuthorId.Should().Be(authorId.Value);
        result.Title.Should().Be("Test Title");
        result.Description.Should().Be("Test Description");
        result.Content.Should().Be("Test Content");
        result.CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(1));
        result.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostReadOnlyRepository(context);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await repository.GetByIdAsync(Guid.NewGuid(), cts.Token));
    }

    [Fact]
    public async Task GetByIdAsync_WithMultiplePosts_ShouldReturnCorrectPost()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostReadOnlyRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Add multiple posts
        var post1Id = PostId.Create(Guid.NewGuid()).Value;
        var post2Id = PostId.Create(Guid.NewGuid()).Value;
        var post3Id = PostId.Create(Guid.NewGuid()).Value;

        var post1 = TestHelpers.CreatePost(post1Id, authorId, "Title 1", "Desc 1", "Content 1", DateTime.UtcNow);
        var post2 = TestHelpers.CreatePost(post2Id, authorId, "Title 2", "Desc 2", "Content 2", DateTime.UtcNow);
        var post3 = TestHelpers.CreatePost(post3Id, authorId, "Title 3", "Desc 3", "Content 3", DateTime.UtcNow);

        context.Posts.AddRange(post1, post2, post3);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(post2Id.Value);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(post2Id.Value);
        result.Title.Should().Be("Title 2");
        result.Description.Should().Be("Desc 2");
        result.Content.Should().Be("Content 2");
    }
}
