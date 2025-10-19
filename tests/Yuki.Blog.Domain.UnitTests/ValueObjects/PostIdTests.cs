using FluentAssertions;
using Xunit;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Domain.UnitTests.ValueObjects;

public class PostIdTests
{
    [Fact]
    public void Create_WithValidGuid_ShouldReturnPostId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = PostId.Create(guid);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(guid);
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldReturnFailure()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var result = PostId.Create(emptyGuid);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Post ID cannot be empty.");
    }

    [Fact]
    public void CreateUnique_ShouldReturnUniquePostId()
    {
        // Act
        var postId1 = PostId.CreateUnique();
        var postId2 = PostId.CreateUnique();

        // Assert
        postId1.Value.Should().NotBe(Guid.Empty);
        postId2.Value.Should().NotBe(Guid.Empty);
        postId1.Should().NotBe(postId2);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var postId = PostId.Create(guid).Value;

        // Act
        var result = postId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var postId1 = PostId.Create(guid).Value;
        var postId2 = PostId.Create(guid).Value;

        // Act & Assert
        postId1.Should().Be(postId2);
        (postId1 == postId2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var postId1 = PostId.CreateUnique();
        var postId2 = PostId.CreateUnique();

        // Act & Assert
        postId1.Should().NotBe(postId2);
        (postId1 != postId2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var postId1 = PostId.Create(guid).Value;
        var postId2 = PostId.Create(guid).Value;

        // Act & Assert
        postId1.GetHashCode().Should().Be(postId2.GetHashCode());
    }

    [Fact]
    public void Value_ShouldReturnUnderlyingGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var postId = PostId.Create(guid).Value;

        // Assert
        postId.Value.Should().Be(guid);
    }
}
