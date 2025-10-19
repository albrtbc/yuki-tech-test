using Yuki.Blog.Domain.Common;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Domain.Entities;

/// <summary>
/// Author aggregate root representing an author in the system.
/// An author can create multiple posts.
/// </summary>
public sealed class Author : AggregateRoot<AuthorId>
{
    /// <summary>
    /// The author's name (first name).
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The author's surname (last name).
    /// </summary>
    public string Surname { get; private set; }

    /// <summary>
    /// The date and time when the author was created in the system.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Author() : base()
    {
        Name = string.Empty;
        Surname = string.Empty;
    }

    private Author(AuthorId id, string name, string surname, DateTime createdAt) : base(id)
    {
        Name = name;
        Surname = surname;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Factory method to create a new Author.
    /// </summary>
    /// <param name="name">The author's first name.</param>
    /// <param name="surname">The author's last name.</param>
    /// <param name="createdAt">The date and time when the author is created.</param>
    /// <returns>A Result containing the new Author if valid, or an error message if invalid.</returns>
    public static DomainResult<Author> Create(string name, string surname, DateTime createdAt)
    {
        // Validate using AuthorName value object
        var authorNameResult = AuthorName.Create(name, surname);
        if (authorNameResult.IsFailure)
        {
            return DomainResult<Author>.Failure(authorNameResult.ErrorMessage);
        }

        var authorId = AuthorId.CreateUnique();
        var author = new Author(authorId, authorNameResult.Value.FirstName, authorNameResult.Value.LastName, createdAt);

        return DomainResult<Author>.Success(author);
    }

    /// <summary>
    /// Factory method to create a new Author with a specific ID (for seeding/testing).
    /// </summary>
    /// <param name="id">The specific author ID.</param>
    /// <param name="name">The author's first name.</param>
    /// <param name="surname">The author's last name.</param>
    /// <param name="createdAt">The date and time when the author is created.</param>
    /// <returns>A Result containing the new Author if valid, or an error message if invalid.</returns>
    public static DomainResult<Author> CreateWithId(Guid id, string name, string surname, DateTime createdAt)
    {
        // Validate using AuthorName value object
        var authorNameResult = AuthorName.Create(name, surname);
        if (authorNameResult.IsFailure)
        {
            return DomainResult<Author>.Failure(authorNameResult.ErrorMessage);
        }

        var authorIdResult = AuthorId.Create(id);
        if (authorIdResult.IsFailure)
        {
            return DomainResult<Author>.Failure(authorIdResult.ErrorMessage);
        }

        var authorId = authorIdResult.Value;
        var author = new Author(authorId, authorNameResult.Value.FirstName, authorNameResult.Value.LastName, createdAt);

        return DomainResult<Author>.Success(author);
    }

    /// <summary>
    /// Updates the author's name.
    /// </summary>
    /// <param name="name">The new first name.</param>
    /// <param name="surname">The new last name.</param>
    /// <returns>A Result indicating success or failure with an error message.</returns>
    public DomainResult UpdateName(string name, string surname)
    {
        // Validate using AuthorName value object
        var authorNameResult = AuthorName.Create(name, surname);
        if (authorNameResult.IsFailure)
        {
            return DomainResult.Failure(authorNameResult.ErrorMessage);
        }

        Name = authorNameResult.Value.FirstName;
        Surname = authorNameResult.Value.LastName;

        return DomainResult.Success();
    }

    /// <summary>
    /// Gets the author's full name.
    /// </summary>
    public string FullName => $"{Name} {Surname}";
}
