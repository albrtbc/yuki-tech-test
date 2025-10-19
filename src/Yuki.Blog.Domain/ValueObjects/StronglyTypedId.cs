using System.Text.RegularExpressions;
using Yuki.Blog.Domain.Common;

namespace Yuki.Blog.Domain.ValueObjects;

/// <summary>
/// Base class for strongly-typed identifiers.
/// Provides common infrastructure for Guid-based IDs while maintaining type safety.
/// </summary>
/// <typeparam name="T">The concrete ID type (e.g., PostId, AuthorId)</typeparam>
public abstract class StronglyTypedId<T> : ValueObject where T : StronglyTypedId<T>
{
    public Guid Value { get; }

    protected StronglyTypedId(Guid value)
    {
        if (value == Guid.Empty)
        {
            var typeName = FormatTypeName(typeof(T).Name);
            throw new ArgumentException($"{typeName} cannot be empty.", nameof(value));
        }

        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    private static string FormatTypeName(string typeName)
    {
        // Add space before capital letters (e.g., "PostId" -> "Post ID")
        return Regex.Replace(typeName, "([A-Z])", " $1").Trim();
    }
}
