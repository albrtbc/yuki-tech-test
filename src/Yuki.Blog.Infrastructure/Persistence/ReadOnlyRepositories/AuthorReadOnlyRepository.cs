using Microsoft.EntityFrameworkCore;
using Yuki.Blog.Domain.ReadOnlyRepositories;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Infrastructure.Persistence.ReadOnlyRepositories;

/// <summary>
/// Read-only repository implementation for Author queries using Entity Framework Core.
/// Projects directly to read-only DTOs for optimal query performance (zero mapping overhead).
/// Uses AuthorReadDto for clear naming convention.
/// Named "ReadOnly" to explicitly indicate no mutations are performed.
/// </summary>
public class AuthorReadOnlyRepository : IAuthorReadOnlyRepository
{
    private readonly BlogDbContext _context;

    public AuthorReadOnlyRepository(BlogDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves an author by ID.
    /// Uses AsNoTracking for read-only query performance.
    /// Projects directly to AuthorReadDto in a single database query.
    /// </summary>
    public async Task<AuthorReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var authorIdResult = AuthorId.Create(id);
        if (authorIdResult.IsFailure)
        {
            return null;
        }

        var authorId = authorIdResult.Value;

        return await _context.Authors
            .AsNoTracking() // Read-only query - no change tracking overhead
            .Where(a => a.Id == authorId)
            .Select(a => new AuthorReadDto
            {
                Id = a.Id.Value,
                Name = a.Name,
                Surname = a.Surname
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
