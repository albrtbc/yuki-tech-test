using FluentAssertions;
using Xunit;
using Yuki.Blog.Application.Features.Posts.Commands.CreatePost;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Application.UnitTests.Features.Posts.Commands.CreatePost;

public class CreatePostCommandValidatorTests
{
    private readonly CreatePostCommandValidator _validator;

    public CreatePostCommandValidatorTests()
    {
        _validator = new CreatePostCommandValidator();
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid description",
            Content = "Valid content"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithEmptyAuthorId_ShouldFail()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.Empty,
            Title = "Valid Title",
            Description = "Valid description",
            Content = "Valid content"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.AuthorId));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Author ID is required"));
    }

    [Fact]
    public async Task Validate_WithEmptyTitle_ShouldFail()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "",
            Description = "Valid description",
            Content = "Valid content"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Title));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Title is required"));
    }

    [Fact]
    public async Task Validate_WithNullTitle_ShouldFail()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = null!,
            Description = "Valid description",
            Content = "Valid content"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Title));
    }

    [Fact]
    public async Task Validate_WithTitleTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = new string('x', PostTitle.MaxLength + 1),
            Description = "Valid description",
            Content = "Valid content"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Title));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("cannot exceed"));
    }

    [Fact]
    public async Task Validate_WithTitleAtMaxLength_ShouldPass()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = new string('x', PostTitle.MaxLength),
            Description = "Valid description",
            Content = "Valid content"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyDescription_ShouldFail()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "",
            Content = "Valid content"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Description));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Description is required"));
    }

    [Fact]
    public async Task Validate_WithNullDescription_ShouldFail()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Valid Title",
            Description = null!,
            Content = "Valid content"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Description));
    }

    [Fact]
    public async Task Validate_WithDescriptionTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Valid Title",
            Description = new string('x', PostDescription.MaxLength + 1),
            Content = "Valid content"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Description));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("cannot exceed"));
    }

    [Fact]
    public async Task Validate_WithDescriptionAtMaxLength_ShouldPass()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Valid Title",
            Description = new string('x', PostDescription.MaxLength),
            Content = "Valid content"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyContent_ShouldFail()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid description",
            Content = ""
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Content));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Content is required"));
    }

    [Fact]
    public async Task Validate_WithNullContent_ShouldFail()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid description",
            Content = null!
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Content));
    }

    [Fact]
    public async Task Validate_WithContentTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid description",
            Content = new string('x', PostContent.MaxLength + 1)
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Content));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("cannot exceed"));
    }

    [Fact]
    public async Task Validate_WithContentAtMaxLength_ShouldPass()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid description",
            Content = new string('x', PostContent.MaxLength)
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.Empty,
            Title = "",
            Description = "",
            Content = ""
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(4);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.AuthorId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Title));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Description));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Content));
    }
}
