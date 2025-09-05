using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Users.Login;

public sealed record LoginCommand : ICommand<UserDto>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

internal sealed class LoginCommandHandler(IApplicationDbContext context, ITokenProvider tokenProvider)
    : ICommandHandler<LoginCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.ToLower();

        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            return Result.Failure<UserDto>(UserErrors.NotFoundByEmail);
        }

        var userDto = user.Adapt<UserDto>();
        userDto.Token = await tokenProvider.CreateAsync(user);

        return userDto;
    }
}
