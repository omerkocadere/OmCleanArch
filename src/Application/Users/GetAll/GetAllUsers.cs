using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Users.DTOs;

namespace CleanArch.Application.Users.GetAll;

public sealed record GetAllUsersQuery : IQuery<IEnumerable<UserDto>>, ICacheableQuery
{
    public string CacheKey => "users:all";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
}

internal sealed class GetAllUsersQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    public async Task<Result<IEnumerable<UserDto>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        var users = await context
            .Users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                DisplayName = u.DisplayName,
                FirstName = u.FirstName,
                LastName = u.LastName,
            })
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<UserDto>>(users);
    }
}
