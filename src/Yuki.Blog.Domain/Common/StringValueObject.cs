namespace Yuki.Blog.Domain.Common;

/// <summary>
/// Base class for string-based value objects.
/// Encapsulates common string validation logic while maintaining DDD principles.
/// Each concrete value object defines its own constraints and validation rules.
/// </summary>
public abstract class StringValueObject : ValueObject
{
    public string Value { get; }

    protected StringValueObject(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Factory method pattern for creating validated instances.
    /// Subclasses must implement this to define their specific validation rules.
    /// </summary>
    protected static DomainResult<string> ValidateString(
        string value,
        string fieldName,
        int minLength,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DomainResult<string>.Failure($"{fieldName} cannot be empty or whitespace.");
        }

        if (value.Length < minLength)
        {
            return DomainResult<string>.Failure($"{fieldName} must be at least {minLength} characters.");
        }

        if (value.Length > maxLength)
        {
            return DomainResult<string>.Failure($"{fieldName} cannot exceed {maxLength} characters.");
        }

        return DomainResult<string>.Success(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
