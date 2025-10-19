using FluentAssertions;
using Xunit;
using Yuki.Blog.Domain.Events;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Domain.UnitTests.Events;

public class PostCreatedEventTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateEvent()
    {
        // Arrange
        var postId = PostId.CreateUnique();
        var authorId = AuthorId.CreateUnique();
        var title = "Test Post Title";
        var occurredOn = DateTime.UtcNow;

        // Act
        var postCreatedEvent = new PostCreatedEvent(postId, authorId, title, occurredOn);

        // Assert
        postCreatedEvent.PostId.Should().Be(postId);
        postCreatedEvent.AuthorId.Should().Be(authorId);
        postCreatedEvent.Title.Should().Be(title);
        postCreatedEvent.OccurredOn.Should().Be(occurredOn);
    }

    [Fact]
    public void Constructor_ShouldStoreProvidedOccurredOn()
    {
        // Arrange
        var postId = PostId.CreateUnique();
        var authorId = AuthorId.CreateUnique();
        var title = "Test Post Title";
        var occurredOn = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var postCreatedEvent = new PostCreatedEvent(postId, authorId, title, occurredOn);

        // Assert
        postCreatedEvent.OccurredOn.Should().Be(occurredOn);
        postCreatedEvent.OccurredOn.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Constructor_WithDifferentData_ShouldCreateDifferentEvents()
    {
        // Arrange
        var postId1 = PostId.CreateUnique();
        var postId2 = PostId.CreateUnique();
        var authorId = AuthorId.CreateUnique();
        var title1 = "First Title";
        var title2 = "Second Title";
        var occurredOn = DateTime.UtcNow;

        // Act
        var event1 = new PostCreatedEvent(postId1, authorId, title1, occurredOn);
        var event2 = new PostCreatedEvent(postId2, authorId, title2, occurredOn);

        // Assert
        event1.PostId.Should().NotBe(event2.PostId);
        event1.Title.Should().NotBe(event2.Title);
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var postId = PostId.CreateUnique();
        var authorId = AuthorId.CreateUnique();
        var title = "Test Title";
        var occurredOn = DateTime.UtcNow;

        // Act
        var postCreatedEvent = new PostCreatedEvent(postId, authorId, title, occurredOn);

        // Assert
        var postIdProperty = typeof(PostCreatedEvent).GetProperty(nameof(PostCreatedEvent.PostId));
        var authorIdProperty = typeof(PostCreatedEvent).GetProperty(nameof(PostCreatedEvent.AuthorId));
        var titleProperty = typeof(PostCreatedEvent).GetProperty(nameof(PostCreatedEvent.Title));
        var occurredOnProperty = typeof(PostCreatedEvent).GetProperty(nameof(PostCreatedEvent.OccurredOn));

        postIdProperty!.CanWrite.Should().BeFalse();
        authorIdProperty!.CanWrite.Should().BeFalse();
        titleProperty!.CanWrite.Should().BeFalse();
        occurredOnProperty!.CanWrite.Should().BeFalse();
    }

    [Fact]
    public void AllProperties_ShouldBeAccessible()
    {
        // Arrange
        var postId = PostId.CreateUnique();
        var authorId = AuthorId.CreateUnique();
        var title = "Test Title";
        var occurredOn = DateTime.UtcNow;

        // Act
        var postCreatedEvent = new PostCreatedEvent(postId, authorId, title, occurredOn);

        // Assert - Ensure all properties are accessible
        postCreatedEvent.PostId.Should().Be(postId);
        postCreatedEvent.AuthorId.Should().Be(authorId);
        postCreatedEvent.Title.Should().Be(title);
        postCreatedEvent.OccurredOn.Should().Be(occurredOn);
    }
}
