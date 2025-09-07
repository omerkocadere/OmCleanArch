using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Users.GetAll;

public sealed record GetAllUsersQuery : IQuery<IEnumerable<UserDto>>, ICacheableQuery
{
    public string CacheKey => "users:all";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}

internal sealed class GetAllUsersQueryHandler(IIdentityService identityService)
    : IQueryHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    public async Task<Result<IEnumerable<UserDto>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        // Single efficient call instead of N+1 query pattern
        var users = await identityService.GetAllUsersAsync();
        return Result.Success<IEnumerable<UserDto>>(users);
    }
}
