using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Users.GetById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDto> { }

internal sealed class GetUserByIdQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IQueryHandler<GetUserByIdQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        if (query.UserId != userContext.UserId)
        {
            return Result.Failure<UserDto>(UserErrors.Unauthorized);
        }

        UserDto? user = await context
            .Users.Where(u => u.Id == query.UserId)
            .ProjectToType<UserDto>()
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>(UserErrors.NotFound(query.UserId));
        }

        return user;
    }
}
