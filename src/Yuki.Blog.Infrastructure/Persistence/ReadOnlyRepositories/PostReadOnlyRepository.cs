using Microsoft.EntityFrameworkCore;
using Yuki.Blog.Domain.ReadOnlyRepositories;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Infrastructure.Persistence.ReadOnlyRepositories;

/// <summary>
/// Read-only repository implementation for Post queries using Entity Framework Core.
/// Projects directly to read-only DTOs for optimal query performance (zero mapping overhead).
/// Uses PostReadDto for clear naming convention.
/// Named "ReadOnly" to explicitly indicate no mutations are performed.
/// </summary>
public class PostReadOnlyRepository : IPostReadOnlyRepository
{
    private readonly BlogDbContext _context;

    public PostReadOnlyRepository(BlogDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a post by ID.
    /// Uses AsNoTracking for read-only query performance.
    /// Projects directly to PostReadDto in a single database query.
    /// Related entities should be fetched via their respective repositories.
    /// </summary>
    public async Task<PostReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Return null for empty GUID instead of throwing exception
        if (id == Guid.Empty)
        {
            return null;
        }

        var postIdResult = PostId.Create(id);
        if (postIdResult.IsFailure)
        {
            return null;
        }

        var postId = postIdResult.Value;

        return await _context.Posts
            .AsNoTracking() // Read-only query - no change tracking overhead
            .Where(p => p.Id == postId)
            .Select(p => new PostReadDto
            {
                Id = p.Id.Value,
                AuthorId = p.AuthorId.Value,
                Title = p.Title.Value,
                Description = p.Description.Value,
                Content = p.Content.Value,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Author = null // No author included - use IAuthorReadOnlyRepository to fetch separately
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
