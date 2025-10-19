using FluentAssertions;
using Xunit;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Domain.UnitTests.ValueObjects;

public class PostContentTests
{
    [Fact]
    public void Create_WithValidContent_ShouldReturnSuccess()
    {
        // Arrange
        var content = "This is valid post content with multiple paragraphs.";

        // Act
        var result = PostContent.Create(content);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(content);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldReturnFailure(string content)
    {
        // Act
        var result = PostContent.Create(content);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Post content cannot be empty or whitespace.");
    }

    [Fact]
    public void Create_WithContentExceedingMaxLength_ShouldReturnFailure()
    {
        // Arrange
        var content = new string('a', PostContent.MaxLength + 1);

        // Act
        var result = PostContent.Create(content);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be($"Post content cannot exceed {PostContent.MaxLength} characters.");
    }

    [Fact]
    public void Create_WithContentAtMaxLength_ShouldReturnSuccess()
    {
        // Arrange
        var content = new string('a', PostContent.MaxLength);

        // Act
        var result = PostContent.Create(content);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(content);
    }

    [Fact]
    public void Create_WithContentAtMinLength_ShouldReturnSuccess()
    {
        // Arrange
        var content = "a";

        // Act
        var result = PostContent.Create(content);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(content);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var content = "Test Content";
        var postContent = PostContent.Create(content).Value;

        // Act
        var result = postContent.ToString();

        // Assert
        result.Should().Be(content);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var content1 = PostContent.Create("Same Content").Value;
        var content2 = PostContent.Create("Same Content").Value;

        // Act & Assert
        content1.Should().Be(content2);
        (content1 == content2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var content1 = PostContent.Create("Content One").Value;
        var content2 = PostContent.Create("Content Two").Value;

        // Act & Assert
        content1.Should().NotBe(content2);
        (content1 != content2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var content1 = PostContent.Create("Same Content").Value;
        var content2 = PostContent.Create("Same Content").Value;

        // Act & Assert
        content1.GetHashCode().Should().Be(content2.GetHashCode());
    }

    [Fact]
    public void Value_ShouldReturnContentString()
    {
        // Arrange
        var contentText = "This is the blog post content with lots of details";

        // Act
        var postContent = PostContent.Create(contentText).Value;

        // Assert
        postContent.Value.Should().Be(contentText);
    }
}
