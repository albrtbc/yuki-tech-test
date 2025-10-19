using FluentAssertions;
using Xunit;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.Events;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Domain.UnitTests.Entities;

public class PostTests
{
    private readonly AuthorId _authorId = AuthorId.CreateUnique();
    private readonly DateTime _createdAt = DateTime.UtcNow;

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var title = "Valid Post Title";
        var description = "Valid post description";
        var content = "Valid post content";

        // Act
        var result = Post.Create(_authorId, title, description, content, _createdAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AuthorId.Should().Be(_authorId);
        result.Value.Title.Value.Should().Be(title);
        result.Value.Description.Value.Should().Be(description);
        result.Value.Content.Value.Should().Be(content);
        result.Value.CreatedAt.Should().Be(_createdAt);
        result.Value.UpdatedAt.Should().BeNull();
        result.Value.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithNullAuthorId_ShouldReturnFailure()
    {
        // Arrange
        var title = "Valid Title";
        var description = "Valid description";
        var content = "Valid content";

        // Act
        var result = Post.Create(null!, title, description, content, _createdAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Author ID cannot be null.");
    }

    [Fact]
    public void Create_WithInvalidTitle_ShouldReturnFailure()
    {
        // Arrange
        var title = "";
        var description = "Valid description";
        var content = "Valid content";

        // Act
        var result = Post.Create(_authorId, title, description, content, _createdAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Post title cannot be empty or whitespace.");
    }

    [Fact]
    public void Create_WithInvalidDescription_ShouldReturnFailure()
    {
        // Arrange
        var title = "Valid Title";
        var description = "";
        var content = "Valid content";

        // Act
        var result = Post.Create(_authorId, title, description, content, _createdAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Post description cannot be empty or whitespace.");
    }

    [Fact]
    public void Create_WithInvalidContent_ShouldReturnFailure()
    {
        // Arrange
        var title = "Valid Title";
        var description = "Valid description";
        var content = "";

        // Act
        var result = Post.Create(_authorId, title, description, content, _createdAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Post content cannot be empty or whitespace.");
    }

    [Fact]
    public void Create_ShouldRaisePostCreatedEvent()
    {
        // Arrange
        var title = "Valid Post Title";
        var description = "Valid post description";
        var content = "Valid post content";

        // Act
        var result = Post.Create(_authorId, title, description, content, _createdAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var post = result.Value;
        post.DomainEvents.Should().HaveCount(1);

        var domainEvent = post.DomainEvents.First();
        domainEvent.Should().BeOfType<PostCreatedEvent>();

        var postCreatedEvent = (PostCreatedEvent)domainEvent;
        postCreatedEvent.PostId.Should().Be(post.Id);
        postCreatedEvent.AuthorId.Should().Be(_authorId);
        postCreatedEvent.Title.Should().Be(title);
    }

    [Fact]
    public void UpdateContent_WithValidContent_ShouldReturnSuccess()
    {
        // Arrange
        var post = Post.Create(_authorId, "Title", "Description", "Original Content", _createdAt).Value;
        var newContent = "Updated Content";
        var updatedAt = DateTime.UtcNow.AddMinutes(10);

        // Act
        var result = post.UpdateContent(newContent, updatedAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        post.Content.Value.Should().Be(newContent);
        post.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void UpdateContent_WithInvalidContent_ShouldReturnFailure()
    {
        // Arrange
        var post = Post.Create(_authorId, "Title", "Description", "Original Content", _createdAt).Value;
        var newContent = "";
        var updatedAt = DateTime.UtcNow.AddMinutes(10);

        // Act
        var result = post.UpdateContent(newContent, updatedAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Post content cannot be empty or whitespace.");
        post.Content.Value.Should().Be("Original Content");
        post.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void UpdateTitleAndDescription_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var post = Post.Create(_authorId, "Original Title", "Original Description", "Content", _createdAt).Value;
        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var updatedAt = DateTime.UtcNow.AddMinutes(10);

        // Act
        var result = post.UpdateTitleAndDescription(newTitle, newDescription, updatedAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        post.Title.Value.Should().Be(newTitle);
        post.Description.Value.Should().Be(newDescription);
        post.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void UpdateTitleAndDescription_WithInvalidTitle_ShouldReturnFailure()
    {
        // Arrange
        var post = Post.Create(_authorId, "Original Title", "Original Description", "Content", _createdAt).Value;
        var newTitle = "";
        var newDescription = "Updated Description";
        var updatedAt = DateTime.UtcNow.AddMinutes(10);

        // Act
        var result = post.UpdateTitleAndDescription(newTitle, newDescription, updatedAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Post title cannot be empty or whitespace.");
        post.Title.Value.Should().Be("Original Title");
        post.Description.Value.Should().Be("Original Description");
        post.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void UpdateTitleAndDescription_WithInvalidDescription_ShouldReturnFailure()
    {
        // Arrange
        var post = Post.Create(_authorId, "Original Title", "Original Description", "Content", _createdAt).Value;
        var newTitle = "Updated Title";
        var newDescription = "";
        var updatedAt = DateTime.UtcNow.AddMinutes(10);

        // Act
        var result = post.UpdateTitleAndDescription(newTitle, newDescription, updatedAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Post description cannot be empty or whitespace.");
        post.Title.Value.Should().Be("Original Title");
        post.Description.Value.Should().Be("Original Description");
        post.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void UpdateContent_ShouldNotAffectTitleOrDescription()
    {
        // Arrange
        var originalTitle = "Original Title";
        var originalDescription = "Original Description";
        var post = Post.Create(_authorId, originalTitle, originalDescription, "Content", _createdAt).Value;
        var updatedAt = DateTime.UtcNow.AddMinutes(10);

        // Act
        post.UpdateContent("New Content", updatedAt);

        // Assert
        post.Title.Value.Should().Be(originalTitle);
        post.Description.Value.Should().Be(originalDescription);
    }

    [Fact]
    public void UpdateTitleAndDescription_ShouldNotAffectContent()
    {
        // Arrange
        var originalContent = "Original Content";
        var post = Post.Create(_authorId, "Title", "Description", originalContent, _createdAt).Value;
        var updatedAt = DateTime.UtcNow.AddMinutes(10);

        // Act
        post.UpdateTitleAndDescription("New Title", "New Description", updatedAt);

        // Assert
        post.Content.Value.Should().Be(originalContent);
    }

    [Fact]
    public void Properties_ShouldBeAccessible()
    {
        // Arrange & Act
        var post = Post.Create(_authorId, "Test Title", "Test Description", "Test Content", _createdAt).Value;

        // Assert - Access all properties to ensure they're covered
        post.Id.Should().NotBeNull();
        post.AuthorId.Should().Be(_authorId);
        post.Title.Should().NotBeNull();
        post.Description.Should().NotBeNull();
        post.Content.Should().NotBeNull();
        post.CreatedAt.Should().Be(_createdAt);
        post.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void UpdatedAt_ShouldBeSetAfterUpdate()
    {
        // Arrange
        var post = Post.Create(_authorId, "Title", "Description", "Content", _createdAt).Value;
        var updateTime = DateTime.UtcNow.AddMinutes(5);

        // Act
        post.UpdateContent("New Content", updateTime);

        // Assert
        post.UpdatedAt.Should().NotBeNull();
        post.UpdatedAt.Should().Be(updateTime);
    }
}
