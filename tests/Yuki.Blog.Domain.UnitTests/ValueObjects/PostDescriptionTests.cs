using FluentAssertions;
using Xunit;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Domain.UnitTests.ValueObjects;

public class PostDescriptionTests
{
    [Fact]
    public void Create_WithValidDescription_ShouldReturnSuccess()
    {
        // Arrange
        var description = "This is a valid post description";

        // Act
        var result = PostDescription.Create(description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldReturnFailure(string description)
    {
        // Act
        var result = PostDescription.Create(description);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Post description cannot be empty or whitespace.");
    }

    [Fact]
    public void Create_WithDescriptionExceedingMaxLength_ShouldReturnFailure()
    {
        // Arrange
        var description = new string('a', PostDescription.MaxLength + 1);

        // Act
        var result = PostDescription.Create(description);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be($"Post description cannot exceed {PostDescription.MaxLength} characters.");
    }

    [Fact]
    public void Create_WithDescriptionAtMaxLength_ShouldReturnSuccess()
    {
        // Arrange
        var description = new string('a', PostDescription.MaxLength);

        // Act
        var result = PostDescription.Create(description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(description);
    }

    [Fact]
    public void Create_WithDescriptionAtMinLength_ShouldReturnSuccess()
    {
        // Arrange
        var description = "a";

        // Act
        var result = PostDescription.Create(description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(description);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var description = "Test Description";
        var postDescription = PostDescription.Create(description).Value;

        // Act
        var result = postDescription.ToString();

        // Assert
        result.Should().Be(description);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var desc1 = PostDescription.Create("Same Description").Value;
        var desc2 = PostDescription.Create("Same Description").Value;

        // Act & Assert
        desc1.Should().Be(desc2);
        (desc1 == desc2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var desc1 = PostDescription.Create("Description One").Value;
        var desc2 = PostDescription.Create("Description Two").Value;

        // Act & Assert
        desc1.Should().NotBe(desc2);
        (desc1 != desc2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var desc1 = PostDescription.Create("Same Description").Value;
        var desc2 = PostDescription.Create("Same Description").Value;

        // Act & Assert
        desc1.GetHashCode().Should().Be(desc2.GetHashCode());
    }

    [Fact]
    public void Value_ShouldReturnDescriptionString()
    {
        // Arrange
        var descText = "This is a description";

        // Act
        var postDescription = PostDescription.Create(descText).Value;

        // Assert
        postDescription.Value.Should().Be(descText);
    }
}
