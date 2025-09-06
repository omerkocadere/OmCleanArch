using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Account.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<UserDto>>;

public sealed class RefreshTokenCommandHandler(IIdentityService identityService, ITokenProvider tokenProvider)
    : IRequestHandler<RefreshTokenCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find user by refresh token
        var user = await identityService.FindByRefreshTokenAsync(request.RefreshToken);

        if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
        {
            return Result.Failure<UserDto>(
                Error.Unauthorized("Authentication.InvalidRefreshToken", "Invalid or expired refresh token")
            );
        }

        // SECURITY: Check absolute 30-day session limit
        if (
            user.RefreshTokenCreatedAt.HasValue
            && DateTime.UtcNow.Subtract(user.RefreshTokenCreatedAt.Value).TotalDays > 30
        )
        {
            // Session exceeded 30 days - force re-authentication
            await InvalidateUserSession(user);
            return Result.Failure<UserDto>(
                Error.Unauthorized("Authentication.SessionExpired", "Session expired. Please log in again.")
            );
        }

        // SECURITY: Token Rotation - Create UserDto with new tokens, preserving RefreshTokenCreatedAt
        return await tokenProvider.CreateUserWithTokensAsync(user, preserveCreatedAt: true);
    }

    private async Task InvalidateUserSession(User user)
    {
        user.RefreshToken = null;
        user.RefreshTokenExpiry = DateTime.UtcNow;
        user.RefreshTokenCreatedAt = null;
        await identityService.UpdateUserAsync(user);
    }
}
