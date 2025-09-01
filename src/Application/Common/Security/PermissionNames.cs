namespace CleanArch.Application.Common.Security;

/// <summary>
/// Permission name constants following KISS principle.
/// Clear separation from domain Permission entity.
/// Cross-cutting concern for authorization across all use cases.
/// </summary>
public static class PermissionNames
{
    // Member Management
    public const string ReadMember = "members:read";
    public const string UpdateMember = "members:update";
    public const string DeleteMember = "members:delete";
    public const string CreateMember = "members:create";

    // User Management
    public const string ReadUser = "users:read";
    public const string UpdateUser = "users:update";
    public const string DeleteUser = "users:delete";
    public const string CreateUser = "users:create";

    // Auction Management
    public const string ReadAuction = "auctions:read";
    public const string UpdateAuction = "auctions:update";
    public const string DeleteAuction = "auctions:delete";
    public const string CreateAuction = "auctions:create";

    // System Administration
    public const string SystemAdmin = "system:admin";
    public const string ViewLogs = "system:logs";

    // Categories - for organization
    public static class Categories
    {
        public const string MemberManagement = "Member Management";
        public const string UserManagement = "User Management";
        public const string AuctionManagement = "Auction Management";
        public const string SystemAdministration = "System Administration";
    }

    // Helper methods - simple and useful
    public static string[] GetAll()
    {
        return
        [
            ReadMember,
            UpdateMember,
            DeleteMember,
            CreateMember,
            ReadUser,
            UpdateUser,
            DeleteUser,
            CreateUser,
            ReadAuction,
            UpdateAuction,
            DeleteAuction,
            CreateAuction,
            SystemAdmin,
            ViewLogs,
        ];
    }

    public static string[] GetMemberPermissions() => [ReadMember, UpdateMember, DeleteMember, CreateMember];

    public static string[] GetUserPermissions() => [ReadUser, UpdateUser, DeleteUser, CreateUser];

    public static string[] GetAuctionPermissions() => [ReadAuction, UpdateAuction, DeleteAuction, CreateAuction];

    public static string[] GetSystemPermissions() => [SystemAdmin, ViewLogs];
}
