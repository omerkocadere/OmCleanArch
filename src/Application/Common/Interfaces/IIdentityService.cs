using CleanArch.Domain.Common;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<IList<string>> GetUserRolesAsync(User user);

    Task<Result> UpdateUserAsync(User user);

    Task<string?> GetUserNameAsync(string userId);

    Task<Result> CreateUserAsync(User user, string password);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<Result> DeleteUserAsync(string userId);

    Task<User?> FindByEmailAsync(string email);

    Task<User?> FindByIdAsync(string userId);

    Task<bool> CheckPasswordAsync(User user, string password);

    Task<Result> AddToRoleAsync(User user, string role);

    Task<Result> AddToRolesAsync(User user, IEnumerable<string> roles);

    Task<Result> RemoveFromRolesAsync(User user, IEnumerable<string> roles);

    Task<List<User>> GetAllUsersAsync();

    Task<User?> FindByRefreshTokenAsync(string refreshToken);
}
