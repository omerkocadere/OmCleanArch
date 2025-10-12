namespace CleanArch.Application.Admin.EditUserRole;

public class EditUserRolesCommandHandler(IIdentityService identityService)
    : ICommandHandler<EditUserRolesCommand, IList<string>>
{
    public async Task<Result<IList<string>>> Handle(EditUserRolesCommand command, CancellationToken cancellationToken)
    {
        var selectedRoles = command
            .Roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(role => role.Trim())
            .ToList();

        return await identityService.UpdateUserRolesAsync(command.UserId, selectedRoles);
    }
}
