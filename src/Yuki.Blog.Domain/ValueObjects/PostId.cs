using Yuki.Blog.Domain.Common;

namespace Yuki.Blog.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for Post entities.
/// Using strongly-typed IDs prevents accidentally passing the wrong ID type to methods.
/// </summary>
public sealed class PostId : StronglyTypedId<PostId>
{
    private PostId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new PostId with the specified Guid value.
    /// </summary>
    public static DomainResult<PostId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return DomainResult<PostId>.Failure("Post ID cannot be empty.");
        }

        return DomainResult<PostId>.Success(new PostId(value));
    }

    /// <summary>
    /// Creates a new PostId with a new Guid value.
    /// </summary>
    public static PostId CreateUnique() => new(Guid.NewGuid());
}
