using CleanArch.Application.Admin.DTOs;

namespace CleanArch.Application.Admin.GetUsersWithRoles;

public sealed record GetUsersWithRolesQuery : IQuery<List<UserWithRolesDto>>;

public class GetUsersWithRolesQueryHandler(IIdentityService identityService)
    : IQueryHandler<GetUsersWithRolesQuery, List<UserWithRolesDto>>
{
    public async Task<Result<List<UserWithRolesDto>>> Handle(
        GetUsersWithRolesQuery request,
        CancellationToken cancellationToken
    )
    {
        var users = await identityService.GetAllUsersAsync();
        var userList = new List<UserWithRolesDto>();

        foreach (var user in users)
        {
            var roles = await identityService.GetUserRolesAsync(user.Id);

            var userWithRoles = new UserWithRolesDto
            {
                Id = user.Id,
                Email = user.Email,
                Roles = roles,
            };

            userList.Add(userWithRoles);
        }

        return userList;
    }
}
