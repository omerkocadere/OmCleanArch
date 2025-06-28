using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArch.Domain.Common;

public abstract class BaseEntity : IEquatable<BaseEntity>
{
    // This can easily be modified to be BaseEntity<T> and public T Id to support different key types.
    // Using non-generic integer types for simplicity
    public int Id { get; private init; }

    private readonly List<BaseEvent> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        return left is not null && right is not null && left.Equals(right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !(left == right);
    }

    public bool Equals(BaseEntity? other)
    {
        if (other is null)
            return false;

        if (other.GetType() != GetType())
            return false;

        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not BaseEntity entity)
            return false;

        if (obj.GetType() != GetType())
            return false;

        return Id == entity.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
