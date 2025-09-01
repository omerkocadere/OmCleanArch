using CleanArch.Domain.Roles;

namespace CleanArch.Domain.Permissions;

public sealed class Permission : BaseAuditableEntity<Guid>
{
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties - Many-to-Many relationship
    public ICollection<Role> Roles { get; set; } = [];
}
