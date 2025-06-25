using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Users.GetByEmail;

public sealed record GetUserByEmailQuery(string Email) : IRequest<Result<UserResponse>>;

internal sealed class GetUserByEmailQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetUserByEmailQuery, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> Handle(
        GetUserByEmailQuery query,
        CancellationToken cancellationToken
    )
    {
        UserResponse? user = await context
            .Users.Where(u => u.Email == query.Email)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFoundByEmail);
        }

        return user;
    }
}
