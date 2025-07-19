using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArch.Domain.Common;

public abstract class BaseEntity<T> : IEquatable<BaseEntity<T>>, IHasDomainEvents
    where T : IEquatable<T>
{
    public T Id { get; set; } = default!;

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

    public static bool operator ==(BaseEntity<T>? left, BaseEntity<T>? right)
    {
        return left is not null && right is not null && left.Equals(right);
    }

    public static bool operator !=(BaseEntity<T>? left, BaseEntity<T>? right)
    {
        return !(left == right);
    }

    public bool Equals(BaseEntity<T>? other)
    {
        if (other is null)
            return false;

        if (other.GetType() != GetType())
            return false;

        return EqualityComparer<T>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not BaseEntity<T> entity)
            return false;

        if (obj.GetType() != GetType())
            return false;

        return EqualityComparer<T>.Default.Equals(Id, entity.Id);
    }

    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }
}
