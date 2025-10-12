namespace CleanArch.Application.Admin.EditUserRole;

public sealed record EditUserRolesCommand(Guid UserId, string Roles) : ICommand<IList<string>>;
