using MediatR;
using Yuki.Blog.Domain.Common;

namespace Yuki.Blog.Application.Common.Interfaces;

/// <summary>
/// Marker interface for domain event handlers.
/// Domain event handlers contain business logic that responds to domain events.
/// Use this for handlers that implement business rules or maintain business invariants.
/// For infrastructure concerns (logging, auditing, notifications), use INotificationHandler directly.
/// </summary>
/// <typeparam name="TEvent">The type of domain event to handle</typeparam>
public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
    // Inherits: Task Handle(TEvent notification, CancellationToken cancellationToken);
}
