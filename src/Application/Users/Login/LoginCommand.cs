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

internal sealed class LoginCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider
) : ICommandHandler<LoginCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<UserDto>(emailResult.Error);
        }

        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == emailResult.Value, cancellationToken);

        if (user == null)
        {
            return Result.Failure<UserDto>(UserErrors.NotFoundByEmail);
        }

        bool verified = passwordHasher.Verify(command.Password, user.PasswordHash);
        if (!verified)
        {
            return Result.Failure<UserDto>(UserErrors.NotFoundByEmail);
        }

        var userDto = user.Adapt<UserDto>();
        userDto.Token = tokenProvider.Create(user);

        return userDto;
    }
}
