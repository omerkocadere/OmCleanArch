using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Users.GetByEmail;

public sealed record GetUserByEmailQuery(string Email) : IQuery<UserDto>;

internal sealed class GetUserByEmailQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetUserByEmailQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        UserDto? user = await context
            .Users.Where(u => u.Email == query.Email)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                DisplayName = u.DisplayName,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>(UserErrors.NotFoundByEmail);
        }

        return user;
    }
}
