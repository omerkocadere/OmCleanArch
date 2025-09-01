using CleanArch.Domain.Members;
using CleanArch.Domain.Roles;

namespace CleanArch.Domain.Users;

public sealed class User : FullAuditableEntity<Guid> // Aggregate Root - ISoftDelete var
{
    private readonly List<Role> _roles = [];

    public required Email Email { get; set; }
    public string? ImageUrl { get; set; }
    public required string DisplayName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PasswordHash { get; set; }

    // Navigation properties
    public required Member Member { get; set; }
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

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
