using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Infrastructure.UnitTests;

/// <summary>
/// Helper methods for tests
/// </summary>
public static class TestHelpers
{
    public static Author CreateAuthor(AuthorId id, string name, string surname, DateTime createdAt)
    {
        var result = Author.CreateWithId(id.Value, name, surname, createdAt);
        if (result.IsFailure)
        {
            throw new Exception($"Failed to create author: {result.ErrorMessage}");
        }
        return result.Value!;
    }

    public static Author CreateAuthor(string name, string surname, DateTime createdAt)
    {
        var result = Author.Create(name, surname, createdAt);
        if (result.IsFailure)
        {
            throw new Exception($"Failed to create author: {result.ErrorMessage}");
        }
        return result.Value!;
    }

    public static Post CreatePost(PostId id, AuthorId authorId, string title, string description, string content, DateTime createdAt)
    {
        var result = Post.CreateWithId(id.Value, authorId, title, description, content, createdAt);
        if (result.IsFailure)
        {
            throw new Exception($"Failed to create post: {result.ErrorMessage}");
        }
        return result.Value!;
    }
}
