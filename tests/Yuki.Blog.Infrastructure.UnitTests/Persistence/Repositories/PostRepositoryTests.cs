using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.ValueObjects;
using Yuki.Blog.Infrastructure.Persistence;
using Yuki.Blog.Infrastructure.Persistence.Repositories;

namespace Yuki.Blog.Infrastructure.UnitTests.Persistence.Repositories;

public class PostRepositoryTests
{
    private readonly Mock<IPublisher> _publisherMock;
    private readonly Mock<ILogger<BlogDbContext>> _loggerMock;

    public PostRepositoryTests()
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
    public async Task GetByIdAsync_WhenPostExists_ShouldReturnPost()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostRepository(context);

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
        var result = await repository.GetByIdAsync(postId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(postId);
        result.AuthorId.Should().Be(authorId);
        result.Title.Value.Should().Be("Test Title");
    }

    [Fact]
    public async Task GetByIdAsync_WhenPostDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostRepository(context);
        var nonExistentId = PostId.Create(Guid.NewGuid()).Value;

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddPostToContext()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var postId = PostId.Create(Guid.NewGuid()).Value;
        var post = TestHelpers.CreatePost(
            postId,
            authorId,
            "Test Title",
            "Test Description",
            "Test Content",
            DateTime.UtcNow);

        // Act
        await repository.AddAsync(post);
        await context.SaveChangesAsync();

        // Assert
        var savedPost = await context.Posts.FindAsync(postId);
        savedPost.Should().NotBeNull();
        savedPost!.Id.Should().Be(postId);
    }

    [Fact]
    public async Task Update_ShouldMarkPostAsModified()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var postId = PostId.Create(Guid.NewGuid()).Value;
        var post = TestHelpers.CreatePost(
            postId,
            authorId,
            "Original Title",
            "Original Description",
            "Original Content",
            DateTime.UtcNow);

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Detach to simulate loading from another context
        context.Entry(post).State = EntityState.Detached;

        var updatedPost = TestHelpers.CreatePost(
            postId,
            authorId,
            "Updated Title",
            "Updated Description",
            "Updated Content",
            DateTime.UtcNow);

        // Act
        repository.Update(updatedPost);
        await context.SaveChangesAsync();

        // Assert
        var savedPost = await context.Posts.FindAsync(postId);
        savedPost.Should().NotBeNull();
        savedPost!.Title.Value.Should().Be("Updated Title");
    }

    [Fact]
    public async Task Remove_ShouldDeletePostFromContext()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var postId = PostId.Create(Guid.NewGuid()).Value;
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
        repository.Remove(post);
        await context.SaveChangesAsync();

        // Assert
        var deletedPost = await context.Posts.FindAsync(postId);
        deletedPost.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WhenPostExists_ShouldReturnTrue()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var postId = PostId.Create(Guid.NewGuid()).Value;
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
        var result = await repository.ExistsAsync(postId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenPostDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostRepository(context);
        var nonExistentId = PostId.Create(Guid.NewGuid()).Value;

        // Act
        var result = await repository.ExistsAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostRepository(context);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await repository.GetByIdAsync(PostId.Create(Guid.NewGuid()).Value, cts.Token));
    }

    [Fact]
    public async Task ExistsAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostRepository(context);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await repository.ExistsAsync(PostId.Create(Guid.NewGuid()).Value, cts.Token));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldEnableChangeTracking()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new PostRepository(context);

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

        // Clear change tracker
        context.ChangeTracker.Clear();

        // Act
        var result = await repository.GetByIdAsync(postId);

        // Assert
        result.Should().NotBeNull();
        context.ChangeTracker.Entries<Post>().Should().NotBeEmpty();
        context.Entry(result!).State.Should().Be(EntityState.Unchanged);
    }
}
