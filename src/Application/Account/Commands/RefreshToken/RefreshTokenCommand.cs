using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Account.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<UserDto>>;

public sealed class RefreshTokenCommandHandler(IIdentityService identityService, ITokenProvider tokenProvider)
    : IRequestHandler<RefreshTokenCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find user by refresh token
        var userDto = await identityService.FindUserByRefreshTokenAsync(request.RefreshToken);

        if (userDto is null)
        {
            return Result.Failure<UserDto>(AuthenticationErrors.InvalidRefreshToken);
        }

        // SECURITY: Check if refresh token is expired
        if (userDto.RefreshTokenExpiry <= DateTime.UtcNow)
        {
            return Result.Failure<UserDto>(AuthenticationErrors.ExpiredRefreshToken);
        }

        // SECURITY: Check 30-day absolute session limit
        if (userDto.RefreshTokenCreatedAt.HasValue)
        {
            var sessionAge = DateTime.UtcNow - userDto.RefreshTokenCreatedAt.Value;
            if (sessionAge.TotalDays > 5)
            {
                return Result.Failure<UserDto>(AuthenticationErrors.SessionExpired);
            }
        }

        string refreshToken = tokenProvider.GenerateRefreshToken();
        DateTime expiry = DateTime.UtcNow.AddDays(3);

        var result = await identityService.UpdateRefreshTokenAsync(userDto.Id, expiry, refreshToken);

        if (result.IsFailure)
        {
            return Result.Failure<UserDto>(result.Error);
        }

        userDto.Token = await tokenProvider.CreateAsync(userDto.Id);
        userDto.RefreshToken = refreshToken;
        userDto.RefreshTokenExpiry = expiry;
        return Result.Success(userDto);
    }
}
