namespace CleanArch.Application.Admin.EditUserRole;

public sealed record EditUserRolesCommand(Guid UserId, string Roles) : ICommand<IList<string>>;

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
