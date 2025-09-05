using CleanArch.Application.Admin.DTOs;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Application.Admin.Queries;

public sealed record GetUsersWithRolesQuery : IQuery<List<UserWithRolesDto>>;

public class GetUsersWithRolesQueryHandler(UserManager<User> userManager)
    : IQueryHandler<GetUsersWithRolesQuery, List<UserWithRolesDto>>
{
    public async Task<Result<List<UserWithRolesDto>>> Handle(
        GetUsersWithRolesQuery request,
        CancellationToken cancellationToken
    )
    {
        var users = await userManager.Users.ToListAsync(cancellationToken);
        var userList = new List<UserWithRolesDto>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var userWithRoles = user.Adapt<UserWithRolesDto>();
            userWithRoles.Roles = roles;
            userList.Add(userWithRoles);
        }

        return userList;
    }
}
