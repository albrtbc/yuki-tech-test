using Yuki.Blog.Domain.Common;

namespace Yuki.Blog.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for Author entities.
/// Using strongly-typed IDs prevents accidentally passing the wrong ID type to methods.
/// </summary>
public sealed class AuthorId : StronglyTypedId<AuthorId>
{
    private AuthorId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new AuthorId with the specified Guid value.
    /// </summary>
    public static DomainResult<AuthorId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return DomainResult<AuthorId>.Failure("Author ID cannot be empty.");
        }

        return DomainResult<AuthorId>.Success(new AuthorId(value));
    }

    /// <summary>
    /// Creates a new AuthorId with a new Guid value.
    /// </summary>
    public static AuthorId CreateUnique() => new(Guid.NewGuid());
}
