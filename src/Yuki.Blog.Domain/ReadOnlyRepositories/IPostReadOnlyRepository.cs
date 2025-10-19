namespace Yuki.Blog.Domain.ReadOnlyRepositories;

/// <summary>
/// Read-only repository interface for Post queries.
/// Separate from IPostRepository which handles commands (write operations).
/// Following CQRS principles: reads are optimized differently from writes.
/// Returns read-only DTOs optimized for queries.
/// Named "ReadOnly" to explicitly indicate no mutations are performed.
/// </summary>
/// <remarks>
/// Implementation will use direct database projections (EF Core Select, Dapper, etc.)
/// to optimize data retrieval without loading full domain aggregates.
/// All methods return DTOs, not domain entities, for optimal query performance.
/// Related entities (like Author) should be fetched via their respective repositories
/// and composed at the application layer for better separation of concerns.
/// </remarks>
public interface IPostReadOnlyRepository
{
    /// <summary>
    /// Retrieves a post by its unique identifier.
    /// Returns a read DTO without related entities.
    /// To include author information, fetch separately via IAuthorReadOnlyRepository.
    /// </summary>
    /// <param name="id">The post ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The post read DTO if found, otherwise null.</returns>
    Task<PostReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Read-only DTO for Post queries.
/// Optimized for data retrieval, not domain behavior.
/// Clearly named with "ReadDto" suffix to indicate read-only query purpose.
/// </summary>
public record PostReadDto
{
    public Guid Id { get; init; }
    public Guid AuthorId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public AuthorReadDto? Author { get; init; }
}
