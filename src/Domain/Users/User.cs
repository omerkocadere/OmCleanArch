using System.ComponentModel.DataAnnotations.Schema;
using CleanArch.Domain.Members;
using CleanArch.Domain.Roles;
using Microsoft.AspNetCore.Identity;

namespace CleanArch.Domain.Users;

public sealed class User : IdentityUser<Guid>, IHasDomainEvents
{
    private readonly List<Role> _roles = [];
    private readonly List<BaseEvent> _domainEvents = [];

    public required string DisplayName { get; set; }
    public string? ImageUrl { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? RefreshToken { get; set; }

    private DateTime? _refreshTokenExpiry;
    public DateTime? RefreshTokenExpiry
    {
        get => _refreshTokenExpiry;
        set => _refreshTokenExpiry = value?.ToUniversalTime(); // Ensure UTC for PostgreSQL
    }

    // Navigation properties
    public required Member Member { get; set; }
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

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

    // Business methods - simple and focused
    public void AssignRole(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        if (!_roles.Contains(role))
        {
            _roles.Add(role);
        }
    }

    public void RemoveRole(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);
        _roles.Remove(role);
    }

    public bool HasRole(string roleName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        return _roles.Any(r => r.Name == roleName);
    }

    public bool HasPermission(string permissionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionName);
        return _roles.Any(r => r.HasPermission(permissionName));
    }

    public IEnumerable<string> GetPermissions()
    {
        return _roles.SelectMany(r => r.Permissions).Select(p => p.Name).Distinct();
    }
}
