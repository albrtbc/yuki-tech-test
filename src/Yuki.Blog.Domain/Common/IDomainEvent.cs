using MediatR;

namespace Yuki.Blog.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain that domain experts care about.
/// Implements INotification to enable MediatR-based event dispatching.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// The date and time when the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}
