namespace CleanArch.Application.Users.Models;

/// <summary>
/// Request model for creating a new user.
/// Encapsulates all parameters needed for user creation to improve code maintainability
/// and comply with clean code principles (max 7 parameters rule).
/// </summary>
public sealed class CreateUserRequest
{
    /// <summary>
    /// The username for the new user. Must be unique.
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// The email address for the new user. Must be unique and valid.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// The password for the new user account.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// The expiry date/time for the refresh token.
    /// </summary>
    public required DateTime RefreshTokenExpiry { get; init; }

    /// <summary>
    /// The refresh token value for authentication.
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// The display name for the user. If null, UserName will be used.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// The first name of the user.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// The last name of the user.
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// The profile image URL for the user.
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// The roles to assign to the new user. Default is empty collection.
    /// </summary>
    public IEnumerable<string> Roles { get; init; } = [];
}
