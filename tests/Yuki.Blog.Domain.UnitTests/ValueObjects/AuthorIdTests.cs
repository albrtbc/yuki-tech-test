using FluentAssertions;
using Xunit;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Domain.UnitTests.ValueObjects;

public class AuthorIdTests
{
    [Fact]
    public void Create_WithValidGuid_ShouldReturnAuthorId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = AuthorId.Create(guid);

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
        var result = AuthorId.Create(emptyGuid);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Author ID cannot be empty.");
    }

    [Fact]
    public void CreateUnique_ShouldReturnUniqueAuthorId()
    {
        // Act
        var authorId1 = AuthorId.CreateUnique();
        var authorId2 = AuthorId.CreateUnique();

        // Assert
        authorId1.Value.Should().NotBe(Guid.Empty);
        authorId2.Value.Should().NotBe(Guid.Empty);
        authorId1.Should().NotBe(authorId2);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var authorId = AuthorId.Create(guid).Value;

        // Act
        var result = authorId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var authorId1 = AuthorId.Create(guid).Value;
        var authorId2 = AuthorId.Create(guid).Value;

        // Act & Assert
        authorId1.Should().Be(authorId2);
        (authorId1 == authorId2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var authorId1 = AuthorId.CreateUnique();
        var authorId2 = AuthorId.CreateUnique();

        // Act & Assert
        authorId1.Should().NotBe(authorId2);
        (authorId1 != authorId2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var authorId1 = AuthorId.Create(guid).Value;
        var authorId2 = AuthorId.Create(guid).Value;

        // Act & Assert
        authorId1.GetHashCode().Should().Be(authorId2.GetHashCode());
    }

    [Fact]
    public void Value_ShouldReturnUnderlyingGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var authorId = AuthorId.Create(guid).Value;

        // Assert
        authorId.Value.Should().Be(guid);
    }
}
