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
        string token = tokenProvider.GenerateRefreshToken();
        DateTime expiry = DateTime.UtcNow.AddDays(3);

        var result = await identityService.Login(command.Password, command.Email, expiry, token);

        if (result.IsFailure)
        {
            return Result.Failure<UserDto>(result.Error);
        }

        result.Value.Token = await tokenProvider.CreateAsync(result.Value.Id);
        return result.Value;
    }
}
