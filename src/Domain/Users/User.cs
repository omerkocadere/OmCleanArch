namespace CleanArch.Domain.Users;

public sealed class User : BaseEntity<Guid>
{
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PasswordHash { get; set; }
}
