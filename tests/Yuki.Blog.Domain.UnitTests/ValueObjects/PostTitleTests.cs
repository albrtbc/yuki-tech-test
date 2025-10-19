using FluentAssertions;
using Xunit;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Domain.UnitTests.ValueObjects;

public class PostTitleTests
{
    [Fact]
    public void Create_WithValidTitle_ShouldReturnSuccess()
    {
        // Arrange
        var title = "Valid Post Title";

        // Act
        var result = PostTitle.Create(title);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldReturnFailure(string title)
    {
        // Act
        var result = PostTitle.Create(title);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Post title cannot be empty or whitespace.");
    }

    [Fact]
    public void Create_WithTitleExceedingMaxLength_ShouldReturnFailure()
    {
        // Arrange
        var title = new string('a', PostTitle.MaxLength + 1);

        // Act
        var result = PostTitle.Create(title);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be($"Post title cannot exceed {PostTitle.MaxLength} characters.");
    }

    [Fact]
    public void Create_WithTitleAtMaxLength_ShouldReturnSuccess()
    {
        // Arrange
        var title = new string('a', PostTitle.MaxLength);

        // Act
        var result = PostTitle.Create(title);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(title);
    }

    [Fact]
    public void Create_WithTitleAtMinLength_ShouldReturnSuccess()
    {
        // Arrange
        var title = "a";

        // Act
        var result = PostTitle.Create(title);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(title);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var title = "Test Title";
        var postTitle = PostTitle.Create(title).Value;

        // Act
        var result = postTitle.ToString();

        // Assert
        result.Should().Be(title);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var title1 = PostTitle.Create("Same Title").Value;
        var title2 = PostTitle.Create("Same Title").Value;

        // Act & Assert
        title1.Should().Be(title2);
        (title1 == title2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var title1 = PostTitle.Create("Title One").Value;
        var title2 = PostTitle.Create("Title Two").Value;

        // Act & Assert
        title1.Should().NotBe(title2);
        (title1 != title2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var title1 = PostTitle.Create("Same Title").Value;
        var title2 = PostTitle.Create("Same Title").Value;

        // Act & Assert
        title1.GetHashCode().Should().Be(title2.GetHashCode());
    }

    [Fact]
    public void Value_ShouldReturnTitleString()
    {
        // Arrange
        var titleText = "My Blog Post Title";

        // Act
        var postTitle = PostTitle.Create(titleText).Value;

        // Assert
        postTitle.Value.Should().Be(titleText);
    }
}
