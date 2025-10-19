using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Yuki.Blog.Application.Features.Posts.EventHandlers;
using Yuki.Blog.Domain.Events;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Application.UnitTests.Features.Posts.EventHandlers;

public class LogPostCreatedHandlerTests
{
    private readonly Mock<ILogger<LogPostCreatedHandler>> _mockLogger;
    private readonly LogPostCreatedHandler _handler;

    public LogPostCreatedHandlerTests()
    {
        _mockLogger = new Mock<ILogger<LogPostCreatedHandler>>();
        _handler = new LogPostCreatedHandler(_mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldLogPostCreatedEvent()
    {
        // Arrange
        var postId = PostId.CreateUnique();
        var authorId = AuthorId.CreateUnique();
        var title = "Test Post Title";
        var occurredOn = DateTime.UtcNow;

        var domainEvent = new PostCreatedEvent(postId, authorId, title, occurredOn);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Domain Event") && v.ToString()!.Contains("Post created")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogPostId()
    {
        // Arrange
        var postId = PostId.CreateUnique();
        var authorId = AuthorId.CreateUnique();
        var title = "Test Post Title";
        var occurredOn = DateTime.UtcNow;

        var domainEvent = new PostCreatedEvent(postId, authorId, title, occurredOn);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(postId.Value.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogPostTitle()
    {
        // Arrange
        var postId = PostId.CreateUnique();
        var authorId = AuthorId.CreateUnique();
        var title = "My Awesome Blog Post";
        var occurredOn = DateTime.UtcNow;

        var domainEvent = new PostCreatedEvent(postId, authorId, title, occurredOn);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(title)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogAuthorId()
    {
        // Arrange
        var postId = PostId.CreateUnique();
        var authorId = AuthorId.CreateUnique();
        var title = "Test Post Title";
        var occurredOn = DateTime.UtcNow;

        var domainEvent = new PostCreatedEvent(postId, authorId, title, occurredOn);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(authorId.Value.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCompleteSuccessfully()
    {
        // Arrange
        var postId = PostId.CreateUnique();
        var authorId = AuthorId.CreateUnique();
        var title = "Test Post Title";
        var occurredOn = DateTime.UtcNow;

        var domainEvent = new PostCreatedEvent(postId, authorId, title, occurredOn);

        // Act
        Func<Task> act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var postId = PostId.CreateUnique();
        var authorId = AuthorId.CreateUnique();
        var title = "Test Post Title";
        var occurredOn = DateTime.UtcNow;

        var domainEvent = new PostCreatedEvent(postId, authorId, title, occurredOn);
        var cancellationToken = new CancellationToken();

        // Act
        Func<Task> act = async () => await _handler.Handle(domainEvent, cancellationToken);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
