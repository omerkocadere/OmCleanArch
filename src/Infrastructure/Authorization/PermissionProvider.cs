using CleanArch.Domain.Roles;
using CleanArch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Authorization;

internal sealed class PermissionProvider(ApplicationDbContext context)
{
    public async Task<HashSet<string>> GetForUserIdAsync(Guid userId)
    {
        IReadOnlyCollection<Role>[] roles = await context
            .Users.Include(x => x.Roles)
            .ThenInclude(x => x.Permissions)
            .Where(x => x.Id == userId)
            .Select(x => x.Roles)
            .ToArrayAsync();

        HashSet<string> permissionsSet =
        [
            .. roles.SelectMany(x => x).SelectMany(x => x.Permissions).Select(x => x.Name),
        ];

        return permissionsSet;
    }
}
