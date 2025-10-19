using FluentAssertions;
using Xunit;
using Yuki.Blog.Application.Features.Posts.Queries.GetPostById;

namespace Yuki.Blog.Application.UnitTests.Features.Posts.Queries.GetPostById;

public class GetPostByIdQueryTests
{
    [Fact]
    public void Constructor_WithPostIdOnly_ShouldSetPostIdAndEmptyIncludes()
    {
        // Arrange
        var postId = Guid.NewGuid();

        // Act
        var query = new GetPostByIdQuery(postId);

        // Assert
        query.PostId.Should().Be(postId);
        query.Includes.Should().BeEmpty();
        query.IncludeAuthor.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithPostIdAndIncludes_ShouldSetBothProperties()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var includes = new[] { "author" };

        // Act
        var query = new GetPostByIdQuery(postId, includes);

        // Assert
        query.PostId.Should().Be(postId);
        query.Includes.Should().BeEquivalentTo(includes);
        query.IncludeAuthor.Should().BeTrue();
    }

    [Fact]
    public void IncludeAuthor_WithAuthorInIncludes_ShouldReturnTrue()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var includes = new[] { "author" };

        // Act
        var query = new GetPostByIdQuery(postId, includes);

        // Assert
        query.IncludeAuthor.Should().BeTrue();
    }

    [Fact]
    public void IncludeAuthor_WithAuthorInIncludesCaseInsensitive_ShouldReturnTrue()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var includes1 = new[] { "AUTHOR" };
        var includes2 = new[] { "Author" };
        var includes3 = new[] { "AuThOr" };

        // Act
        var query1 = new GetPostByIdQuery(postId, includes1);
        var query2 = new GetPostByIdQuery(postId, includes2);
        var query3 = new GetPostByIdQuery(postId, includes3);

        // Assert
        query1.IncludeAuthor.Should().BeTrue();
        query2.IncludeAuthor.Should().BeTrue();
        query3.IncludeAuthor.Should().BeTrue();
    }

    [Fact]
    public void IncludeAuthor_WithoutAuthorInIncludes_ShouldReturnFalse()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var includes = new[] { "comments", "tags" };

        // Act
        var query = new GetPostByIdQuery(postId, includes);

        // Assert
        query.IncludeAuthor.Should().BeFalse();
    }

    [Fact]
    public void IncludeAuthor_WithEmptyIncludes_ShouldReturnFalse()
    {
        // Arrange
        var postId = Guid.NewGuid();

        // Act
        var query = new GetPostByIdQuery(postId);

        // Assert
        query.IncludeAuthor.Should().BeFalse();
    }

    [Fact]
    public void IncludeAuthor_WithNullIncludes_ShouldReturnFalse()
    {
        // Arrange
        var postId = Guid.NewGuid();

        // Act
        var query = new GetPostByIdQuery(postId, null);

        // Assert
        query.IncludeAuthor.Should().BeFalse();
        query.Includes.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithMultipleIncludes_ShouldStoreAll()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var includes = new[] { "author", "comments", "tags" };

        // Act
        var query = new GetPostByIdQuery(postId, includes);

        // Assert
        query.Includes.Should().HaveCount(3);
        query.Includes.Should().Contain("author");
        query.Includes.Should().Contain("comments");
        query.Includes.Should().Contain("tags");
    }

    [Fact]
    public void Includes_ShouldBeImmutableReadOnlyList()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var includes = new[] { "author" };

        // Act
        var query = new GetPostByIdQuery(postId, includes);

        // Assert - Includes should be a read-only list
        query.Includes.Should().BeAssignableTo<IReadOnlyList<string>>();
        query.Includes.Should().HaveCount(1);
        query.Includes.Should().Contain("author");
    }

    [Fact]
    public void AsRecord_ShouldSupportValueEquality()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var includes = new[] { "author" };
        var query1 = new GetPostByIdQuery(postId, includes);
        var query2 = new GetPostByIdQuery(postId, includes);

        // Act & Assert
        query1.Should().Be(query2);
        (query1 == query2).Should().BeTrue();
    }

    [Fact]
    public void AsRecord_WithDifferentPostIds_ShouldNotBeEqual()
    {
        // Arrange
        var query1 = new GetPostByIdQuery(Guid.NewGuid());
        var query2 = new GetPostByIdQuery(Guid.NewGuid());

        // Act & Assert
        query1.Should().NotBe(query2);
    }
}
