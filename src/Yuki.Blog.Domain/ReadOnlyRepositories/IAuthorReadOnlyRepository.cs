namespace Yuki.Blog.Domain.ReadOnlyRepositories;

/// <summary>
/// Read-only repository interface for Author queries.
/// Separate from IAuthorRepository which handles commands (write operations).
/// Following CQRS principles: reads are optimized differently from writes.
/// Returns read-only DTOs optimized for queries.
/// Named "ReadOnly" to explicitly indicate no mutations are performed.
/// </summary>
/// <remarks>
/// Implementation will use direct database projections (EF Core Select, Dapper, etc.)
/// to optimize data retrieval without loading full domain aggregates.
/// All methods return DTOs, not domain entities, for optimal query performance.
/// </remarks>
public interface IAuthorReadOnlyRepository
{
    /// <summary>
    /// Retrieves an author by their unique identifier.
    /// Returns a read DTO without related entities.
    /// </summary>
    /// <param name="id">The author ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The author read DTO if found, otherwise null.</returns>
    Task<AuthorReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Read-only DTO for Author data in queries.
/// Optimized for data retrieval, not domain behavior.
/// Clearly named with "ReadDto" suffix to indicate read-only query purpose.
/// </summary>
public record AuthorReadDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Surname { get; init; } = string.Empty;
    public string FullName => $"{Name} {Surname}".Trim();
}
