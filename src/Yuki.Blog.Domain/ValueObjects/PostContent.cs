using Yuki.Blog.Domain.Common;

namespace Yuki.Blog.Domain.ValueObjects;

/// <summary>
/// Value object representing the content of a blog post.
/// Encapsulates validation logic for post content.
/// </summary>
public sealed class PostContent : StringValueObject
{
    public const int MaxLength = 50000;
    public const int MinLength = 1;

    private PostContent(string value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new PostContent instance with validation.
    /// </summary>
    /// <param name="value">The content text.</param>
    /// <returns>A Result containing the PostContent if valid, or an error message if invalid.</returns>
    public static DomainResult<PostContent> Create(string value)
    {
        var validationResult = ValidateString(value, "Post content", MinLength, MaxLength);
        if (validationResult.IsFailure)
        {
            return DomainResult<PostContent>.Failure(validationResult.ErrorMessage);
        }

        return DomainResult<PostContent>.Success(new PostContent(validationResult.Value));
    }
}
