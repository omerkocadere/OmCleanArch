using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<bool> CheckPasswordAsync(Guid userId, string password);
    Task<UserDto?> FindUserByRefreshTokenAsync(string refreshToken);
    Task<(Result Result, UserDto? UserDto)> CreateUserAsync(
        string userName,
        string email,
        string password,
        string? displayName = null,
        string? firstName = null,
        string? lastName = null,
        string? imageUrl = null
    );
    Task<Result<IList<string>>> UpdateUserRolesAsync(Guid userId, IEnumerable<string> newRoles);

    Task<UserDto?> GetUserByIdAsync(Guid userId);

    Task<IList<string>> GetUserRolesAsync(Guid userId);

    Task<Result> UpdateUserAsync(
        Guid userId,
        string? displayName = null,
        string? firstName = null,
        string? lastName = null,
        string? imageUrl = null
    );

    Task<Result> AddToRolesAsync(Guid userId, IEnumerable<string> roles);

    Task<Result> UpdateRefreshTokenAsync(Guid userId, string? refreshToken, DateTime? expiry, DateTime? createdAt);

    // Efficient method to get all users at once (replaces N+1 query pattern)
    Task<List<UserDto>> GetAllUsersAsync();

    // Direct email lookup method (replaces two-step query pattern)
    Task<UserDto?> GetUserByEmailAsync(string email);

    // Methods for seeding operations
    Task<bool> HasAnyUsersAsync();
}
