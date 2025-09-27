using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Users.GetById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDto> { }

internal sealed class GetUserByIdQueryHandler(IIdentityService identityService, IUserContext userContext)
    : IQueryHandler<GetUserByIdQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        if (query.UserId != userContext.UserId)
        {
            return Result.Failure<UserDto>(UserErrors.Unauthorized);
        }

        var user = await identityService.GetUserByIdAsync(query.UserId);

        if (user is null)
        {
            return Result.Failure<UserDto>(UserErrors.NotFound(query.UserId));
        }

        return user;
    }
}
