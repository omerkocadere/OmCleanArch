using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Infrastructure.Options;
using Mapster;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CleanArch.Infrastructure.Authentication;

internal sealed class TokenProvider(IOptions<AuthenticationOptions> authOptions, IIdentityService identityService)
    : ITokenProvider
{
    private readonly JwtOptions _jwtOptions = authOptions.Value.Jwt;

    public async Task<string> CreateAsync(Guid userId)
    {
        if (string.IsNullOrWhiteSpace(_jwtOptions.Secret))
            throw new InvalidOperationException("JWT Secret is not configured");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = await CreateClaims(userId);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return handler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    private async Task<List<Claim>> CreateClaims(Guid userId)
    {
        var user = await identityService.GetUserByIdAsync(userId);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, user?.Email ?? string.Empty),
        };

        var roles = await identityService.GetUserRolesAsync(userId);
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        return claims;
    }

    // public async Task<Result<UserDto>> CreateUserWithTokensAsync(Guid userId, bool preserveCreatedAt = false)
    // {
    //     // Set refresh token
    //     var refreshTokenResult = await SetRefreshTokenAsync(userId, preserveCreatedAt);
    //     if (refreshTokenResult.IsFailure)
    //     {
    //         return Result.Failure<UserDto>(refreshTokenResult.Error);
    //     }

    //     var (refreshToken, expiry) = refreshTokenResult.Value;

    //     // Get user details
    //     var userDto = await identityService.GetUserByIdAsync(userId);
    //     if (userDto == null)
    //     {
    //         return Result.Failure<UserDto>(UserErrors.NotFound(userId));
    //     }

    //     // Add tokens to UserDto
    //     userDto.Token = await CreateAsync(userId);
    //     userDto.RefreshToken = refreshToken;
    //     userDto.RefreshTokenExpiry = expiry;

    //     return Result.Success(userDto);
    // }

    // private async Task<Result<(string refreshToken, DateTime expiry)>> SetRefreshTokenAsync(
    //     Guid userId,
    //     bool preserveCreatedAt = false
    // )
    // {
    //     var refreshToken = GenerateRefreshToken();
    //     var expiry = DateTime.UtcNow.AddDays(3);
    //     var createdAt = preserveCreatedAt ? (DateTime?)null : DateTime.UtcNow;

    //     var updateResult = await identityService.UpdateRefreshTokenAsync(userId, refreshToken, expiry, createdAt);
    //     if (updateResult.IsFailure)
    //     {
    //         return Result.Failure<(string refreshToken, DateTime expiry)>(updateResult.Error);
    //     }

    //     return Result.Success((refreshToken, expiry));
    // }
}
