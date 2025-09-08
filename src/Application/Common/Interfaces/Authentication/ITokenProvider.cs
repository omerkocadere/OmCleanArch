using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Common.Interfaces.Authentication;

public interface ITokenProvider
{
    Task<string> CreateAsync(Guid userId);
    public string GenerateRefreshToken();
    // Task<Result<(string refreshToken, DateTime expiry)>> SetRefreshTokenAsync(
    //     Guid userId,
    //     bool preserveCreatedAt = false
    // );
    // Task<Result<UserDto>> CreateUserWithTokensAsync(Guid userId, bool preserveCreatedAt = false);
}
