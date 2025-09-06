using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;
using CleanArch.Infrastructure.Options;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CleanArch.Infrastructure.Authentication;

internal sealed class TokenProvider(IOptions<AuthenticationOptions> authOptions, UserManager<User> userManager)
    : ITokenProvider
{
    private readonly JwtOptions _jwtOptions = authOptions.Value.Jwt;

    public async Task<string> CreateAsync(User user)
    {
        if (string.IsNullOrWhiteSpace(_jwtOptions.Secret))
            throw new InvalidOperationException("JWT Secret is not configured");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = await CreateClaims(user);

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

    private async Task<List<Claim>> CreateClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        return claims;
    }

    public async Task<Result<UserDto>> CreateUserWithTokensAsync(User user, bool preserveCreatedAt = false)
    {
        // Set refresh token
        var refreshTokenResult = await SetRefreshTokenAsync(user, preserveCreatedAt);
        if (refreshTokenResult.IsFailure)
        {
            return Result.Failure<UserDto>(refreshTokenResult.Error);
        }

        var (refreshToken, expiry) = refreshTokenResult.Value;

        // Create UserDto with all tokens
        var userDto = user.Adapt<UserDto>();
        userDto.Token = await CreateAsync(user);
        userDto.RefreshToken = refreshToken;
        userDto.RefreshTokenExpiry = expiry;

        return Result.Success(userDto);
    }

    private async Task<Result<(string refreshToken, DateTime expiry)>> SetRefreshTokenAsync(
        User user,
        bool preserveCreatedAt = false
    )
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(3);

        if (!preserveCreatedAt)
        {
            user.RefreshTokenCreatedAt = DateTime.UtcNow;
        }
        // If preserveCreatedAt = true, keep original RefreshTokenCreatedAt to maintain absolute session limit

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToArray();
            var validationError = new ValidationError(errors);
            return Result.Failure<(string refreshToken, DateTime expiry)>(validationError);
        }

        return Result.Success((refreshToken, user.RefreshTokenExpiry.Value));
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}
