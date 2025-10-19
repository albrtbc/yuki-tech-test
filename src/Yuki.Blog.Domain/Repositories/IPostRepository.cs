using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Domain.Repositories;

/// <summary>
/// Repository interface for Post aggregate root.
/// Focuses on command operations (write/validation) only.
/// Read operations for queries should use direct database projections via IUnitOfWork.
/// Following CQRS principles: Commands use domain layer, Queries bypass it.
/// </summary>
public interface IPostRepository
{
    /// <summary>
    /// Adds a new post to the repository.
    /// Used for command operations (Create).
    /// </summary>
    /// <param name="post">The post to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(Post post, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing post in the repository.
    /// Used for command operations (Update).
    /// </summary>
    /// <param name="post">The post to update.</param>
    void Update(Post post);

    /// <summary>
    /// Removes a post from the repository.
    /// Used for command operations (Delete).
    /// </summary>
    /// <param name="post">The post to remove.</param>
    void Remove(Post post);

    /// <summary>
    /// Checks if a post with the given ID exists.
    /// Used for validation in command operations.
    /// Note: This is kept because it's used for business rule validation, not data retrieval.
    /// </summary>
    /// <param name="id">The post ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the post exists, otherwise false.</returns>
    Task<bool> ExistsAsync(PostId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a post by its unique identifier for modification.
    /// Only used in command handlers when we need to load aggregate for updates.
    /// For read-only queries, use direct database projections.
    /// </summary>
    /// <param name="id">The post ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The post if found, otherwise null.</returns>
    Task<Post?> GetByIdAsync(PostId id, CancellationToken cancellationToken = default);
}
