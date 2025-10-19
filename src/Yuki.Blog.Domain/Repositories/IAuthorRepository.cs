using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Domain.Repositories;

/// <summary>
/// Repository interface for Author aggregate root.
/// Defines the contract for data access operations on authors.
/// Following the Repository pattern from DDD.
/// </summary>
public interface IAuthorRepository
{
    /// <summary>
    /// Retrieves an author by their unique identifier.
    /// </summary>
    /// <param name="id">The author ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The author if found, otherwise null.</returns>
    Task<Author?> GetByIdAsync(AuthorId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new author to the repository.
    /// </summary>
    /// <param name="author">The author to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(Author author, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing author in the repository.
    /// </summary>
    /// <param name="author">The author to update.</param>
    void Update(Author author);

    /// <summary>
    /// Removes an author from the repository.
    /// </summary>
    /// <param name="author">The author to remove.</param>
    void Remove(Author author);

    /// <summary>
    /// Checks if an author with the given ID exists.
    /// </summary>
    /// <param name="id">The author ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the author exists, otherwise false.</returns>
    Task<bool> ExistsAsync(AuthorId id, CancellationToken cancellationToken = default);
}
