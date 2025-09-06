using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace CleanArch.Application.Account.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<UserDto>>;

public sealed class RefreshTokenCommandHandler(UserManager<User> userManager, ITokenProvider tokenProvider)
    : IRequestHandler<RefreshTokenCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find user by refresh token
        var user = await userManager.Users.FirstOrDefaultAsync(
            x => x.RefreshToken == request.RefreshToken && x.RefreshTokenExpiry > DateTime.UtcNow,
            cancellationToken
        );

        if (user == null)
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
        await userManager.UpdateAsync(user);
    }
}
