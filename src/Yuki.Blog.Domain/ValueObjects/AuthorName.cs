using Yuki.Blog.Domain.Common;

namespace Yuki.Blog.Domain.ValueObjects;

/// <summary>
/// Value object representing an author's full name.
/// Encapsulates validation logic for author names.
/// </summary>
public sealed class AuthorName : ValueObject
{
    public const int MaxLength = 100;
    public const int MinLength = 1;

    public string FirstName { get; }
    public string LastName { get; }

    private AuthorName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    /// <summary>
    /// Creates a new AuthorName instance with validation.
    /// </summary>
    /// <param name="firstName">The author's first name.</param>
    /// <param name="lastName">The author's last name.</param>
    /// <returns>A Result containing the AuthorName if valid, or an error message if invalid.</returns>
    public static DomainResult<AuthorName> Create(string firstName, string lastName)
    {
        // Validate first name
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return DomainResult<AuthorName>.Failure("First name cannot be empty or whitespace.");
        }

        if (firstName.Length < MinLength)
        {
            return DomainResult<AuthorName>.Failure($"First name must be at least {MinLength} characters.");
        }

        if (firstName.Length > MaxLength)
        {
            return DomainResult<AuthorName>.Failure($"First name cannot exceed {MaxLength} characters.");
        }

        // Validate last name
        if (string.IsNullOrWhiteSpace(lastName))
        {
            return DomainResult<AuthorName>.Failure("Last name cannot be empty or whitespace.");
        }

        if (lastName.Length < MinLength)
        {
            return DomainResult<AuthorName>.Failure($"Last name must be at least {MinLength} characters.");
        }

        if (lastName.Length > MaxLength)
        {
            return DomainResult<AuthorName>.Failure($"Last name cannot exceed {MaxLength} characters.");
        }

        return DomainResult<AuthorName>.Success(new AuthorName(firstName, lastName));
    }

    public string FullName => $"{FirstName} {LastName}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }

    public override string ToString() => FullName;
}
