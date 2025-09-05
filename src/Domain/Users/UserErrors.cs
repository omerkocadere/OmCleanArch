namespace CleanArch.Domain.Users;

public static class UserErrors
{
    public static readonly Error Forbidden = Error.Forbidden(
        "Users.Forbidden",
        "You do not have permission to perform this action."
    );

    public static Error NotFound(Guid userId) =>
        Error.NotFound("Users.NotFound", $"The user with the Id = '{userId}' was not found");


    public static readonly Error Unauthorized = Error.Unauthorized(
        "Users.Unauthorized",
        "You are not authorized to perform this action."
    );

    public static readonly Error NotFoundByEmail = Error.Problem(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found"
    );

    public static readonly Error RolesRequired = Error.Validation(
        "Users.RolesRequired",
        "You must select at least one role"
    );

    public static readonly Error RoleAssignmentFailed = Error.Problem(
        "Users.RoleAssignmentFailed",
        "Failed to assign roles to user"
    );

    public static readonly Error RoleRemovalFailed = Error.Problem(
        "Users.RoleRemovalFailed",
        "Failed to remove roles from user"
    );

    public static readonly Error CreationFailed = Error.Problem("Users.CreationFailed", "Failed to create user");
}
