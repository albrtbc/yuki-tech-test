using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuki.Blog.Application.Common.Interfaces;
using Yuki.Blog.Domain.Entities;

namespace Yuki.Blog.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for the Blog application.
/// Implements IUnitOfWork for transaction management.
/// </summary>
public class BlogDbContext : DbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;
    private readonly ILogger<BlogDbContext> _logger;

    public BlogDbContext(
        DbContextOptions<BlogDbContext> options,
        IPublisher publisher,
        ILogger<BlogDbContext> logger) : base(options)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Author> Authors => Set<Author>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect all domain events from all aggregate roots before saving
        var domainEvents = GetDomainEventsFromAggregates();

        // Clear domain events before saving (prevents duplication if SaveChanges is called again)
        ClearDomainEventsFromAggregates();

        // Save changes to database
        var result = await base.SaveChangesAsync(cancellationToken);

        // Publish domain events in parallel after successful save
        // This ensures data is persisted before side effects and improves performance
        await PublishDomainEventsAsync(domainEvents, cancellationToken);

        return result;
    }

    /// <summary>
    /// Publishes domain events in parallel with proper error handling.
    /// One failing handler won't prevent other handlers from executing.
    /// </summary>
    private async Task PublishDomainEventsAsync(
        List<Domain.Common.IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        if (!domainEvents.Any())
        {
            return;
        }

        // Create a task for each event publication with individual error handling
        var publishTasks = domainEvents.Select(async domainEvent =>
        {
            try
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - allows other handlers to continue
                _logger.LogError(
                    ex,
                    "Error publishing domain event {EventType}. Event details: {@Event}",
                    domainEvent.GetType().Name,
                    domainEvent);
            }
        });

        // Execute all publish tasks in parallel
        await Task.WhenAll(publishTasks);
    }

    /// <summary>
    /// Collects domain events from all aggregate roots in the change tracker.
    /// </summary>
    private List<Domain.Common.IDomainEvent> GetDomainEventsFromAggregates()
    {
        return ChangeTracker.Entries<Domain.Common.AggregateRoot<Domain.ValueObjects.PostId>>()
            .Select(e => e.Entity)
            .SelectMany(e => e.DomainEvents)
            .Concat(
                ChangeTracker.Entries<Domain.Common.AggregateRoot<Domain.ValueObjects.AuthorId>>()
                    .Select(e => e.Entity)
                    .SelectMany(e => e.DomainEvents)
            )
            .ToList();
    }

    /// <summary>
    /// Clears domain events from all aggregate roots in the change tracker.
    /// </summary>
    private void ClearDomainEventsFromAggregates()
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.AggregateRoot<Domain.ValueObjects.PostId>>())
        {
            entry.Entity.ClearDomainEvents();
        }

        foreach (var entry in ChangeTracker.Entries<Domain.Common.AggregateRoot<Domain.ValueObjects.AuthorId>>())
        {
            entry.Entity.ClearDomainEvents();
        }
    }
}
