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

/// <summary>
/// Contains constant values for authorization policies used throughout the application.
/// This prevents magic strings and provides compile-time safety for policy names.
/// </summary>
public static class AuthorizationPolicies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string MemberOnly = nameof(MemberOnly);
    public const string ModeratorOnly = nameof(ModeratorOnly);
    public const string ModeratePhotoRole = nameof(ModeratePhotoRole);
}
