using CleanArch.Application.Users.DTOs;

namespace CleanArch.Application.Account.Login;

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
