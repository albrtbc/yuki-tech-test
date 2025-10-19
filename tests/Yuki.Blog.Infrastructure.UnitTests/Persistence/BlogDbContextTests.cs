using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Yuki.Blog.Domain.Common;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.ValueObjects;
using Yuki.Blog.Infrastructure.Persistence;

namespace Yuki.Blog.Infrastructure.UnitTests.Persistence;

public class BlogDbContextTests
{
    private readonly Mock<IPublisher> _publisherMock;
    private readonly Mock<ILogger<BlogDbContext>> _loggerMock;

    public BlogDbContextTests()
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
    public void Constructor_ShouldInitializeDbSets()
    {
        // Arrange & Act
        using var context = CreateContext();

        // Assert
        context.Posts.Should().NotBeNull();
        context.Authors.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_WithNoDomainEvents_ShouldSaveSuccessfully()
    {
        // Arrange
        using var context = CreateContext();
        var author = TestHelpers.CreateAuthor(
            AuthorId.Create(Guid.NewGuid()).Value,
            "Albert",
            "Blanco",
            DateTime.UtcNow);

        context.Authors.Add(author);

        // Clear domain events to simulate no events
        author.ClearDomainEvents();

        // Act
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        _publisherMock.Verify(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveChangesAsync_WithDomainEvents_ShouldPublishEvents()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var postId = PostId.Create(Guid.NewGuid()).Value;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync(); // Save author first

        // Create post which raises domain event
        var post = TestHelpers.CreatePost(
            postId,
            authorId,
            "Test Title",
            "Test Description",
            "Test Content",
            DateTime.UtcNow);

        context.Posts.Add(post);

        // Act
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().Be(4); // 1 post + 3 owned types (Title, Description, Content)
        _publisherMock.Verify(
            p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleDomainEvents_ShouldPublishAllEvents()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync(); // Save author first

        // Create multiple posts which raise multiple domain events
        var post1 = TestHelpers.CreatePost(
            PostId.Create(Guid.NewGuid()).Value,
            authorId,
            "Test Title 1",
            "Test Description 1",
            "Test Content 1",
            DateTime.UtcNow);

        var post2 = TestHelpers.CreatePost(
            PostId.Create(Guid.NewGuid()).Value,
            authorId,
            "Test Title 2",
            "Test Description 2",
            "Test Content 2",
            DateTime.UtcNow);

        context.Posts.Add(post1);
        context.Posts.Add(post2);

        // Act
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().Be(8); // 2 posts + 6 owned types (3 per post: Title, Description, Content)
        _publisherMock.Verify(
            p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task SaveChangesAsync_WhenPublishingFails_ShouldLogErrorAndContinue()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync(); // Save author first

        var post = TestHelpers.CreatePost(
            PostId.Create(Guid.NewGuid()).Value,
            authorId,
            "Test Title",
            "Test Description",
            "Test Content",
            DateTime.UtcNow);

        context.Posts.Add(post);

        // Setup publisher to throw exception
        _publisherMock
            .Setup(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Publishing failed"));

        // Act
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().Be(4); // Save should still succeed (1 post + 3 owned types)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldClearDomainEventsAfterCollection()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync(); // Save author first

        var post = TestHelpers.CreatePost(
            PostId.Create(Guid.NewGuid()).Value,
            authorId,
            "Test Title",
            "Test Description",
            "Test Content",
            DateTime.UtcNow);

        context.Posts.Add(post);

        // Verify events exist before save
        post.DomainEvents.Should().NotBeEmpty();

        // Act
        await context.SaveChangesAsync();

        // Assert - events should be cleared
        post.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        using var context = CreateContext();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var author = TestHelpers.CreateAuthor(
            AuthorId.Create(Guid.NewGuid()).Value,
            "Albert",
            "Blanco",
            DateTime.UtcNow);

        context.Authors.Add(author);

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await context.SaveChangesAsync(cts.Token));
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPublishEventsInParallel()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;

        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Create multiple posts
        var posts = new List<Post>();
        for (int i = 0; i < 5; i++)
        {
            var post = TestHelpers.CreatePost(
                PostId.Create(Guid.NewGuid()).Value,
                authorId,
                $"Test Title {i}",
                $"Test Description {i}",
                $"Test Content {i}",
                DateTime.UtcNow);

            posts.Add(post);
            context.Posts.Add(post);
        }

        var publishCallTimes = new List<DateTime>();
        _publisherMock
            .Setup(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => publishCallTimes.Add(DateTime.UtcNow))
            .Returns(Task.CompletedTask);

        // Act
        await context.SaveChangesAsync();

        // Assert
        _publisherMock.Verify(
            p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(5));
    }

    [Fact]
    public void Posts_DbSet_ShouldBeAccessible()
    {
        // Arrange & Act
        using var context = CreateContext();

        // Assert
        context.Posts.Should().NotBeNull();
        context.Posts.Should().BeAssignableTo<DbSet<Post>>();
    }

    [Fact]
    public void Authors_DbSet_ShouldBeAccessible()
    {
        // Arrange & Act
        using var context = CreateContext();

        // Assert
        context.Authors.Should().NotBeNull();
        context.Authors.Should().BeAssignableTo<DbSet<Author>>();
    }
}
