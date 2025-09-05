using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace CleanArch.Application.Admin.Commands;

public sealed record EditUserRolesCommand(Guid UserId, string Roles) : ICommand<IList<string>>;

public class EditUserRolesCommandHandler(UserManager<User> userManager)
    : ICommandHandler<EditUserRolesCommand, IList<string>>
{
    public async Task<Result<IList<string>>> Handle(EditUserRolesCommand command, CancellationToken cancellationToken)
    {
        var selectedRoles = command
            .Roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(role => role.Trim())
            .ToArray();

        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
        {
            return Result.Failure<IList<string>>(UserErrors.NotFound(command.UserId));
        }

        var userRoles = await userManager.GetRolesAsync(user);

        // Add new roles that user doesn't have
        var rolesToAdd = selectedRoles.Except(userRoles);
        if (rolesToAdd.Any())
        {
            var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                return Result.Failure<IList<string>>(UserErrors.RoleAssignmentFailed);
            }
        }

        // Remove roles that user has but are not selected
        var rolesToRemove = userRoles.Except(selectedRoles);
        if (rolesToRemove.Any())
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                return Result.Failure<IList<string>>(UserErrors.RoleRemovalFailed);
            }
        }

        var finalRoles = await userManager.GetRolesAsync(user);
        return Result.Success(finalRoles);
    }
}
