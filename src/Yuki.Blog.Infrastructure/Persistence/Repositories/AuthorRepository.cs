using Microsoft.EntityFrameworkCore;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.Repositories;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementation of IAuthorRepository using Entity Framework Core.
/// Inherits from TracedRepository to provide OpenTelemetry tracing for all operations.
/// </summary>
public class AuthorRepository : TracedRepository, IAuthorRepository
{
    private readonly BlogDbContext _context;

    public AuthorRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<Author?> GetByIdAsync(AuthorId id, CancellationToken cancellationToken = default)
    {
        return await TraceAsync(
            "GetAuthorById",
            async () => await _context.Authors.FirstOrDefaultAsync(a => a.Id == id, cancellationToken),
            activity => activity?.SetTag("author.id", id.Value));
    }

    public async Task AddAsync(Author author, CancellationToken cancellationToken = default)
    {
        await TraceAsync(
            "AddAuthor",
            async () => await _context.Authors.AddAsync(author, cancellationToken),
            activity =>
            {
                activity?.SetTag("author.id", author.Id.Value);
                activity?.SetTag("author.name", author.FullName);
            });
    }

    public void Update(Author author)
    {
        using var activity = System.Diagnostics.Activity.Current?.Source.StartActivity("Repository.UpdateAuthor");
        activity?.SetTag("repository.operation", "UpdateAuthor");
        activity?.SetTag("author.id", author.Id.Value);
        _context.Authors.Update(author);
    }

    public void Remove(Author author)
    {
        using var activity = System.Diagnostics.Activity.Current?.Source.StartActivity("Repository.RemoveAuthor");
        activity?.SetTag("repository.operation", "RemoveAuthor");
        activity?.SetTag("author.id", author.Id.Value);
        _context.Authors.Remove(author);
    }

    public async Task<bool> ExistsAsync(AuthorId id, CancellationToken cancellationToken = default)
    {
        return await TraceAsync(
            "AuthorExists",
            async () => await _context.Authors.AnyAsync(a => a.Id == id, cancellationToken),
            activity => activity?.SetTag("author.id", id.Value));
    }
}
