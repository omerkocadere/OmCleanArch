using System.ComponentModel.DataAnnotations.Schema;
using CleanArch.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace CleanArch.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>, IHasDomainEvents
{
    private readonly List<BaseEvent> _domainEvents = [];

    public required string DisplayName { get; set; }
    public string? ImageUrl { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime? RefreshTokenCreatedAt { get; set; }

    // Note: Relationship with Member maintained through shared ID (ApplicationUser.Id == Member.Id)
    // No navigation property to avoid FK constraint issues
    // Use manual queries when you need to access related Member: context.Members.Find(user.Id)

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
