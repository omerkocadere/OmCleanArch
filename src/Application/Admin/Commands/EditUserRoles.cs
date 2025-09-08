using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;

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
            .ToList();

        return await identityService.UpdateUserRolesAsync(command.UserId, selectedRoles);
    }
}
