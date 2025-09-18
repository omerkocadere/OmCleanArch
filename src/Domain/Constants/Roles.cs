namespace CleanArch.Domain.Constants;

public static class UserRoles
{
    public const string Member = nameof(Member);
    public const string Moderator = nameof(Moderator);
    public const string Admin = nameof(Admin);
    public static readonly string[] All = [Member, Moderator, Admin];
}
