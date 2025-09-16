namespace CleanArch.Domain.Constants;

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
