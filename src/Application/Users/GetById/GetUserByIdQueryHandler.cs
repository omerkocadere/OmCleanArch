using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Users.GetById;

public sealed record GetUserByIdQuery(int UserId) : IRequest<Result<UserDto>>;

internal sealed class GetUserByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(
        GetUserByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        UserDto? user = await context
            .Users.Where(u => u.Id == query.UserId)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>(UserErrors.NotFound(query.UserId));
        }

        return user;
    }
}
