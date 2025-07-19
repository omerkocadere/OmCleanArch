namespace CleanArch.Domain.Users;

public sealed class User : BaseEntity<Guid>
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PasswordHash { get; set; }

    // Private constructor to ensure creation through factory method
    private User() { }

    public static User Create(string email, string firstName, string lastName, string passwordHash)
    {
        // Basic validation
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName, nameof(firstName));
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName, nameof(lastName));
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash, nameof(passwordHash));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            PasswordHash = passwordHash,
        };

        // Add domain event
        user.AddDomainEvent(new UserCreatedDomainEvent(user));

        return user;
    }
}
