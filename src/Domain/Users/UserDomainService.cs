namespace CleanArch.Domain.Users;

/// <summary>
/// Domain service for User entity business rules.
/// </summary>
public static class UserDomainService
{
    public static Result CanDeleteUser(
        int userIdToDelete,
        int currentUserId,
        bool isSystemAdministrator,
        bool hasActivePurchaseOrders
    )
    {
        // Business Rule: A user cannot delete themselves
        if (userIdToDelete == currentUserId)
        {
            return Result.Failure(UserErrors.SelfDelete);
        }

        // Business Rule: The system administrator cannot be deleted (system integrity)
        if (isSystemAdministrator)
        {
            return Result.Failure(UserErrors.CannotDeleteSystemAdmin);
        }

        // Business Rule: A user cannot be deleted if they have active purchase orders assigned
        if (hasActivePurchaseOrders)
        {
            return Result.Failure(UserErrors.HasActivePurchaseOrders);
        }

        return Result.Success();
    }
}
