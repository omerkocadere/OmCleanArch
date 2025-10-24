namespace CleanArch.Application.Common.Errors;

public static class RegistrationErrors
{
    public static Error UserCreationFailed(string details) =>
        Error.Problem("Registration.UserCreationFailed", $"Failed to create user account: {details}");

    public static Error RoleAssignmentFailed(string role) =>
        Error.Problem("Registration.RoleAssignmentFailed", $"Failed to assign role '{role}' to user");

    public static Error MemberCreationFailed(string details) =>
        Error.Problem("Registration.MemberCreationFailed", $"Failed to create member profile: {details}");

    public static Error TransactionFailed(string details) =>
        Error.Problem("Registration.TransactionFailed", $"Registration transaction failed: {details}");

    public static Error DomainEventFailed =>
        Error.Problem("Registration.DomainEventFailed", "Failed to add domain event for user registration");
}
