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
}
