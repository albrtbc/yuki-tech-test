namespace Yuki.Blog.Application.Common.Interfaces;

/// <summary>
/// Represents a unit of work for managing database transactions.
/// Follows the Unit of Work pattern from Martin Fowler's PoEAA.
/// Responsible for coordinating writes and maintaining consistency across aggregates.
/// </summary>
/// <remarks>
/// For read operations, use read-only repositories (e.g., IPostReadOnlyRepository, IAuthorReadOnlyRepository).
/// This keeps the unit of work focused on transaction management, not data access.
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in the current unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
