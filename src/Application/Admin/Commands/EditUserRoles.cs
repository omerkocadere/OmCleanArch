using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Admin.Commands;

public sealed record EditUserRolesCommand(Guid UserId, string Roles) : ICommand<IList<string>>;

public class EditUserRolesCommandHandler(IIdentityService identityService)
    : ICommandHandler<EditUserRolesCommand, IList<string>>
{
    public async Task<Result<IList<string>>> Handle(EditUserRolesCommand command, CancellationToken cancellationToken)
    {
        var selectedRoles = command
            .Roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(role => role.Trim())
            .ToArray();

        var user = await identityService.FindByIdAsync(command.UserId.ToString());
        if (user == null)
        {
            return Result.Failure<IList<string>>(UserErrors.NotFound(command.UserId));
        }

        var userRoles = await identityService.GetUserRolesAsync(user);

        // Add new roles that user doesn't have
        var rolesToAdd = selectedRoles.Except(userRoles);
        if (rolesToAdd.Any())
        {
            var addResult = await identityService.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.IsSuccess)
            {
                return Result.Failure<IList<string>>(UserErrors.RoleAssignmentFailed);
            }
        }

        // Remove roles that user has but are not selected
        var rolesToRemove = userRoles.Except(selectedRoles);
        if (rolesToRemove.Any())
        {
            var removeResult = await identityService.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.IsSuccess)
            {
                return Result.Failure<IList<string>>(UserErrors.RoleRemovalFailed);
            }
        }

        var finalRoles = await identityService.GetUserRolesAsync(user);
        return Result.Success(finalRoles);
    }
}
