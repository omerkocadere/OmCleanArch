using CleanArch.Domain.Constants;

namespace CleanArch.Application.Admin.Commands;

public class EditUserRolesCommandValidator : AbstractValidator<EditUserRolesCommand>
{
    public EditUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Roles)
            .NotEmpty()
            .WithMessage("Roles are required.")
            .NotNull()
            .WithMessage("Roles cannot be null.")
            .Must(BeValidRoleFormat)
            .WithMessage("Roles must be provided in comma-separated format.")
            .Must(ContainOnlyValidRoles)
            .WithMessage($"Roles must be one of: {string.Join(", ", UserRoles.All)}");
    }

    private static bool BeValidRoleFormat(string roles)
    {
        if (string.IsNullOrWhiteSpace(roles))
            return false;

        // Check if it contains at least one role after splitting
        var roleArray = roles.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return roleArray.Length > 0 && roleArray.All(role => !string.IsNullOrWhiteSpace(role.Trim()));
    }

    private static bool ContainOnlyValidRoles(string roles)
    {
        if (string.IsNullOrWhiteSpace(roles))
            return false;

        var selectedRoles = roles
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(role => role.Trim())
            .ToArray();

        // Check if all selected roles are valid
        return selectedRoles.All(role => UserRoles.All.Contains(role));
    }
}
