using Yuki.Blog.Domain.Common;

namespace Yuki.Blog.Domain.ValueObjects;

/// <summary>
/// Value object representing the description of a blog post.
/// Encapsulates validation logic for post description.
/// </summary>
public sealed class PostDescription : StringValueObject
{
    public const int MaxLength = 500;
    public const int MinLength = 1;

    private PostDescription(string value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new PostDescription instance with validation.
    /// </summary>
    /// <param name="value">The description text.</param>
    /// <returns>A Result containing the PostDescription if valid, or an error message if invalid.</returns>
    public static DomainResult<PostDescription> Create(string value)
    {
        var validationResult = ValidateString(value, "Post description", MinLength, MaxLength);
        if (validationResult.IsFailure)
        {
            return DomainResult<PostDescription>.Failure(validationResult.ErrorMessage);
        }

        return DomainResult<PostDescription>.Success(new PostDescription(validationResult.Value));
    }
}
