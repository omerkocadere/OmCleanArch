using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Users.GetByEmail;

public sealed record GetUserByEmailQuery(string Email) : IQuery<UserDto>;

internal sealed class GetUserByEmailQueryHandler(IIdentityService identityService)
    : IQueryHandler<GetUserByEmailQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        // Single efficient call instead of two-step query
        var user = await identityService.GetUserByEmailAsync(query.Email);

        if (user == null)
        {
            return Result.Failure<UserDto>(UserErrors.NotFoundByEmail);
        }

        return user;
    }
}
