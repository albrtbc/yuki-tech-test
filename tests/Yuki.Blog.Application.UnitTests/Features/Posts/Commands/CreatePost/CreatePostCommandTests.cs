using FluentAssertions;
using Xunit;
using Yuki.Blog.Application.Features.Posts.Commands.CreatePost;

namespace Yuki.Blog.Application.UnitTests.Features.Posts.Commands.CreatePost;

public class CreatePostCommandTests
{
    [Fact]
    public void CreatePostCommand_ShouldInitializeWithDefaultValues()
    {
        // Act
        var command = new CreatePostCommand();

        // Assert
        command.AuthorId.Should().Be(Guid.Empty);
        command.Title.Should().BeEmpty();
        command.Description.Should().BeEmpty();
        command.Content.Should().BeEmpty();
    }

    [Fact]
    public void CreatePostCommand_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var title = "Test Title";
        var description = "Test Description";
        var content = "Test Content";

        // Act
        var command = new CreatePostCommand
        {
            AuthorId = authorId,
            Title = title,
            Description = description,
            Content = content
        };

        // Assert
        command.AuthorId.Should().Be(authorId);
        command.Title.Should().Be(title);
        command.Description.Should().Be(description);
        command.Content.Should().Be(content);
    }

    [Fact]
    public void CreatePostCommand_AsRecord_ShouldSupportValueEquality()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var command1 = new CreatePostCommand
        {
            AuthorId = authorId,
            Title = "Title",
            Description = "Description",
            Content = "Content"
        };

        var command2 = new CreatePostCommand
        {
            AuthorId = authorId,
            Title = "Title",
            Description = "Description",
            Content = "Content"
        };

        // Act & Assert
        command1.Should().Be(command2);
        (command1 == command2).Should().BeTrue();
    }

    [Fact]
    public void CreatePostCommand_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var command1 = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Title1",
            Description = "Description",
            Content = "Content"
        };

        var command2 = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Title2",
            Description = "Description",
            Content = "Content"
        };

        // Act & Assert
        command1.Should().NotBe(command2);
    }

    [Fact]
    public void CreatePostCommand_ShouldSupportWith()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Original Title",
            Description = "Description",
            Content = "Content"
        };

        // Act
        var modifiedCommand = command with { Title = "Modified Title" };

        // Assert
        modifiedCommand.Title.Should().Be("Modified Title");
        modifiedCommand.AuthorId.Should().Be(command.AuthorId);
        modifiedCommand.Description.Should().Be(command.Description);
        modifiedCommand.Content.Should().Be(command.Content);
        command.Title.Should().Be("Original Title"); // Original unchanged
    }
}
