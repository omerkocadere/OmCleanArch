namespace CleanArch.Domain.Constants;

public static class AuthorizationPolicies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string MemberOnly = nameof(MemberOnly);
    public const string ModeratorOnly = nameof(ModeratorOnly);
    public const string ModeratePhotoRole = nameof(ModeratePhotoRole);
}
