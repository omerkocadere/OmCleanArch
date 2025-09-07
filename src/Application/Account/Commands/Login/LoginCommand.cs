using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Account.Commands.Login;

public sealed record LoginCommand : ICommand<UserDto>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

internal sealed class LoginCommandHandler(IIdentityService identityService, ITokenProvider tokenProvider)
    : ICommandHandler<LoginCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var userDto = await identityService.GetUserByEmailAsync(command.Email);
        if (userDto is null)
        {
            return Result.Failure<UserDto>(AuthenticationErrors.InvalidCredentials);
        }

        var result = await identityService.CheckPasswordAsync(userDto.Id, command.Password);
        if (!result)
        {
            return Result.Failure<UserDto>(AuthenticationErrors.InvalidCredentials);
        }

        // Create UserDto with tokens using centralized method
        return await tokenProvider.CreateUserWithTokensAsync(userDto.Id);
    }
}
