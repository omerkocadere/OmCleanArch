using CleanArch.Domain.Permissions;
using CleanArch.Domain.Users;

namespace CleanArch.Domain.Roles;

public sealed class Role : BaseAuditableEntity<Guid>
{
    private readonly List<Permission> _permissions = [];
    private readonly List<User> _users = [];

    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public string? Description { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // Read-only access to collections - KISS principle
    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    // Business methods - simple and focused
    public void AddPermission(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        if (!_permissions.Contains(permission))
        {
            _permissions.Add(permission);
        }
    }

    public void RemovePermission(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);
        _permissions.Remove(permission);
    }

    public bool HasPermission(string permissionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionName);
        return _permissions.Any(p => p.Name == permissionName);
    }

    // Simple validation - no over-engineering
    public bool IsValidRole()
    {
        return !string.IsNullOrWhiteSpace(Name) && IsActive;
    }
}
