namespace CleanArch.Domain.Users;

/// <summary>
/// Represents a user in the system.
/// This interface allows domain entities to reference users without depending on Infrastructure layer.
/// </summary>
public interface IUser
{
    int Id { get; }
    string? UserName { get; } // Nullable - matches IdentityUser
    string FirstName { get; }
    string LastName { get; }
    string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Gets the user's initials (e.g., "John Doe" -> "JD")
    /// Used for displaying compact user identifiers in UI (avatars, badges)
    /// </summary>
    string Initials =>
        $"{(FirstName.Length > 0 ? FirstName[0].ToString() : "")}{(LastName.Length > 0 ? LastName[0].ToString() : "")}".ToUpper();

    /// <summary>
    /// Gets the user's display name for formal communications
    /// Prefers FullName if available, falls back to UserName
    /// </summary>
    string DisplayName => !string.IsNullOrWhiteSpace(FullName) ? FullName : UserName ?? "Unknown User";
}
