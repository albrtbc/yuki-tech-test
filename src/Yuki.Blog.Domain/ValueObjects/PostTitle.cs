using Yuki.Blog.Domain.Common;

namespace Yuki.Blog.Domain.ValueObjects;

/// <summary>
/// Value object representing the title of a blog post.
/// Encapsulates validation logic for post title.
/// </summary>
public sealed class PostTitle : StringValueObject
{
    public const int MaxLength = 200;
    public const int MinLength = 1;

    private PostTitle(string value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new PostTitle instance with validation.
    /// </summary>
    /// <param name="value">The title text.</param>
    /// <returns>A Result containing the PostTitle if valid, or an error message if invalid.</returns>
    public static DomainResult<PostTitle> Create(string value)
    {
        var validationResult = ValidateString(value, "Post title", MinLength, MaxLength);
        if (validationResult.IsFailure)
        {
            return DomainResult<PostTitle>.Failure(validationResult.ErrorMessage);
        }

        return DomainResult<PostTitle>.Success(new PostTitle(validationResult.Value));
    }
}
