using CleanArch.Domain.Members;

namespace CleanArch.Domain.Users;

public sealed class User : FullAuditableEntity<Guid>
{
    public required string Email { get; set; }
    public string? ImageUrl { get; set; }
    public required string DisplayName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PasswordHash { get; set; }

    // Navigation property
    public required Member Member { get; set; }
}
