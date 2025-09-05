using System.ComponentModel.DataAnnotations.Schema;
using CleanArch.Domain.Members;
using Microsoft.AspNetCore.Identity;

namespace CleanArch.Domain.Users;

public sealed class User : IdentityUser<Guid>, IHasDomainEvents
{
    private readonly List<BaseEvent> _domainEvents = [];

    public required string DisplayName { get; set; }
    public string? ImageUrl { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? RefreshToken { get; set; }

    private DateTime? _refreshTokenExpiry;
    public DateTime? RefreshTokenExpiry
    {
        get => _refreshTokenExpiry;
        set => _refreshTokenExpiry = value?.ToUniversalTime(); // Ensure UTC for PostgreSQL
    }

    // Navigation properties
    public Member Member { get; set; } = null!;

    // Domain Events implementation
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
}
