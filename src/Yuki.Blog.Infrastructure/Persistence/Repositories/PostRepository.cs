using Microsoft.EntityFrameworkCore;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.Repositories;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementation of IPostRepository using Entity Framework Core.
/// Focuses on command operations only (write/validation).
/// Query operations should use direct database projections via IUnitOfWork.
/// Inherits from TracedRepository to provide OpenTelemetry tracing for all operations.
/// </summary>
public class PostRepository : TracedRepository, IPostRepository
{
    private readonly BlogDbContext _context;

    public PostRepository(BlogDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a post by ID for modification in command handlers.
    /// This loads the full aggregate with change tracking enabled.
    /// </summary>
    public async Task<Post?> GetByIdAsync(PostId id, CancellationToken cancellationToken = default)
    {
        return await TraceAsync(
            "GetPostById",
            async () => await _context.Posts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken),
            activity => activity?.SetTag("post.id", id.Value));
    }

    /// <summary>
    /// Adds a new post to the database.
    /// </summary>
    public async Task AddAsync(Post post, CancellationToken cancellationToken = default)
    {
        await TraceAsync(
            "AddPost",
            async () => await _context.Posts.AddAsync(post, cancellationToken),
            activity =>
            {
                activity?.SetTag("post.id", post.Id.Value);
                activity?.SetTag("post.title", post.Title.Value);
                activity?.SetTag("post.author_id", post.AuthorId.Value);
            });
    }

    /// <summary>
    /// Marks an existing post for update.
    /// </summary>
    public void Update(Post post)
    {
        // Sync operation - trace it inline
        using var activity = System.Diagnostics.Activity.Current?.Source.StartActivity("Repository.UpdatePost");
        activity?.SetTag("repository.operation", "UpdatePost");
        activity?.SetTag("repository.type", GetType().Name);
        activity?.SetTag("post.id", post.Id.Value);
        activity?.SetTag("post.title", post.Title.Value);

        _context.Posts.Update(post);
    }

    /// <summary>
    /// Removes a post from the database.
    /// </summary>
    public void Remove(Post post)
    {
        // Sync operation - trace it inline
        using var activity = System.Diagnostics.Activity.Current?.Source.StartActivity("Repository.RemovePost");
        activity?.SetTag("repository.operation", "RemovePost");
        activity?.SetTag("repository.type", GetType().Name);
        activity?.SetTag("post.id", post.Id.Value);

        _context.Posts.Remove(post);
    }

    /// <summary>
    /// Checks if a post exists (used for validation in commands).
    /// </summary>
    public async Task<bool> ExistsAsync(PostId id, CancellationToken cancellationToken = default)
    {
        return await TraceAsync(
            "PostExists",
            async () => await _context.Posts.AnyAsync(p => p.Id == id, cancellationToken),
            activity => activity?.SetTag("post.id", id.Value));
    }
}
