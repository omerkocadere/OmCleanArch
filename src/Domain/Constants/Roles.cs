namespace CleanArch.Domain.Constants;

/// <summary>
/// Contains constant values for user roles used throughout the application.
/// This prevents magic strings and provides compile-time safety for role names.
/// </summary>
public static class UserRoles
{
    public const string Member = nameof(Member);
    public const string Moderator = nameof(Moderator);
    public const string Admin = nameof(Admin);
    public static readonly string[] All = [Member, Moderator, Admin];
}


