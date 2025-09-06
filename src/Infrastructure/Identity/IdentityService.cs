using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Identity;

internal sealed class IdentityService(UserManager<User> userManager) : IIdentityService
{
    public async Task<IList<string>> GetUserRolesAsync(User user)
    {
        return await userManager.GetRolesAsync(user);
    }

    public async Task<Result> UpdateUserAsync(User user)
    {
        var result = await userManager.UpdateAsync(user);

        return result.Succeeded ? Result.Success() : Result.Failure(ConvertIdentityErrors(result.Errors));
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user?.UserName;
    }

    public async Task<Result> CreateUserAsync(User user, string password)
    {
        var result = await userManager.CreateAsync(user, password);

        return result.Succeeded ? Result.Success() : Result.Failure(ConvertIdentityErrors(result.Errors));
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user != null && await userManager.IsInRoleAsync(user, role);
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Success(); // User not found, consider it already deleted
        }

        var result = await userManager.DeleteAsync(user);

        return result.Succeeded ? Result.Success() : Result.Failure(ConvertIdentityErrors(result.Errors));
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await userManager.FindByEmailAsync(email);
    }

    public async Task<User?> FindByIdAsync(string userId)
    {
        return await userManager.FindByIdAsync(userId);
    }

    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        return await userManager.CheckPasswordAsync(user, password);
    }

    public async Task<Result> AddToRoleAsync(User user, string role)
    {
        var result = await userManager.AddToRoleAsync(user, role);

        return result.Succeeded ? Result.Success() : Result.Failure(ConvertIdentityErrors(result.Errors));
    }

    public async Task<Result> AddToRolesAsync(User user, IEnumerable<string> roles)
    {
        var result = await userManager.AddToRolesAsync(user, roles);

        return result.Succeeded ? Result.Success() : Result.Failure(ConvertIdentityErrors(result.Errors));
    }

    public async Task<Result> RemoveFromRolesAsync(User user, IEnumerable<string> roles)
    {
        var result = await userManager.RemoveFromRolesAsync(user, roles);

        return result.Succeeded ? Result.Success() : Result.Failure(ConvertIdentityErrors(result.Errors));
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await userManager.Users.ToListAsync();
    }

    public async Task<User?> FindByRefreshTokenAsync(string refreshToken)
    {
        return await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    /// <summary>
    /// Converts IdentityError collection to ValidationError for Result pattern.
    /// </summary>
    private static ValidationError ConvertIdentityErrors(IEnumerable<IdentityError> identityErrors)
    {
        var errors = identityErrors.Select(e => Error.Validation(e.Code, e.Description)).ToArray();

        return new ValidationError(errors);
    }
}
