using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<UserDto>> Login(string password, string email, string refreshToken);

    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<List<UserDto>> GetAllUsersAsync();

    Task<UserDto?> FindUserByRefreshTokenAsync(string refreshToken);
    Task<Result> UpdateRefreshTokenAsync(Guid userId, DateTime expiry, string? refreshToken);

    Task<IList<string>> GetUserRolesAsync(Guid userId);
    Task<Result<IList<string>>> UpdateUserRolesAsync(Guid userId, List<string> newRoles);

    Task<Result<UserDto>> CreateUserAsync(
        string userName,
        string email,
        string password,
        string refreshToken,
        string? displayName = null,
        string? firstName = null,
        string? lastName = null,
        string? imageUrl = null,
        IEnumerable<string> roles = null!
    );

    Task<Result> UpdateUserAsync(
        Guid userId,
        string? displayName = null,
        string? firstName = null,
        string? lastName = null,
        string? imageUrl = null
    );

    Task<bool> HasAnyUsersAsync();
}
