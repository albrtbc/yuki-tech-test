namespace Yuki.Blog.Domain.Common;

/// <summary>
/// Base class for all entities in the domain.
/// Entities have a unique identity that runs through time and different representations.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// The unique identifier of the entity.
    /// </summary>
    public TId Id { get; protected set; }

    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Protected parameterless constructor for EF Core.
    /// </summary>
    protected Entity()
    {
        Id = default!;
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Id.Equals(entity.Id);
    }

    public bool Equals(Entity<TId>? other)
    {
        return Equals((object?)other);
    }

    public static bool operator ==(Entity<TId> left, Entity<TId> right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId> left, Entity<TId> right)
    {
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
