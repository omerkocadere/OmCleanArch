using CleanArch.Domain.Permissions;
using CleanArch.Domain.Users;

namespace CleanArch.Domain.Roles;

public sealed class Role : BaseAuditableEntity<Guid>
{
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public string? Description { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
}
