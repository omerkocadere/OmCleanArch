using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Users.DTOs;
using CleanArch.Application.Users.Models;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Identity;

internal sealed class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<Result<UserDto>> Login(string password, string email, DateTime expiry, string refreshToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure<UserDto>(AuthenticationErrors.InvalidCredentials);
        }

        var isSuccess = await userManager.CheckPasswordAsync(user, password);
        if (!isSuccess)
        {
            return Result.Failure<UserDto>(AuthenticationErrors.InvalidCredentials);
        }

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = expiry;
        user.RefreshTokenCreatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);

        return result.Succeeded ? user.Adapt<UserDto>() : Result.Failure<UserDto>(ConvertIdentityErrors(result.Errors));
    }

    public async Task<Result<UserDto>> CreateUserAsync(CreateUserRequest request)
    {
        // Create ApplicationUser entity
        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            DisplayName = request.DisplayName ?? request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            ImageUrl = request.ImageUrl,
            RefreshToken = request.RefreshToken,
            RefreshTokenExpiry = request.RefreshTokenExpiry,
            RefreshTokenCreatedAt = DateTime.UtcNow,
        };

        // Create User with Identity
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Result.Failure<UserDto>(ConvertIdentityErrors(result.Errors));
        }

        result = await userManager.AddToRolesAsync(user, request.Roles);
        if (!result.Succeeded)
        {
            return Result.Failure<UserDto>(ConvertIdentityErrors(result.Errors));
        }

        // Note: Domain events will be added in the calling handler where SaveChanges is called
        // This ensures the SaveChanges interceptor can properly process the events

        return user.Adapt<UserDto>();
    }

    public async Task<UserDto?> FindUserByRefreshTokenAsync(string refreshToken)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        return user?.Adapt<UserDto>();
    }

    public async Task<Result> InvalidateRefreshTokenAsync(string refreshToken)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user == null)
            return Result.Success(); // Token already invalid/doesn't exist

        user.RefreshToken = null;
        user.RefreshTokenExpiry = DateTime.UtcNow; // Expired
        user.RefreshTokenCreatedAt = null;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded ? Result.Success() : Result.Failure(ConvertIdentityErrors(result.Errors));
    }

    public async Task<Result> UpdateRefreshTokenAsync(Guid userId, DateTime expiry, string? refreshToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result.Failure(UserErrors.NotFound(userId));

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = expiry;
        user.RefreshTokenCreatedAt = refreshToken is null ? null : user.RefreshTokenCreatedAt;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded ? Result.Success() : Result.Failure(ConvertIdentityErrors(result.Errors));
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return null;

        return user.Adapt<UserDto>();
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user?.Adapt<UserDto>();
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await userManager.Users.ToListAsync();
        return users.Adapt<List<UserDto>>();
    }

    public async Task<Result> UpdateUserAsync(
        Guid userId,
        string? displayName = null,
        string? firstName = null,
        string? lastName = null,
        string? imageUrl = null
    )
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result.Failure(UserErrors.NotFound(userId));

        user.DisplayName = displayName ?? user.DisplayName;
        user.FirstName = firstName ?? user.FirstName;
        user.LastName = lastName ?? user.LastName;
        user.ImageUrl = imageUrl ?? user.ImageUrl;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded ? Result.Success() : Result.Failure(ConvertIdentityErrors(result.Errors));
    }

    public async Task<IList<string>> GetUserRolesAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user != null ? await userManager.GetRolesAsync(user) : [];
    }

    public async Task<bool> HasAnyUsersAsync()
    {
        return await userManager.Users.AnyAsync();
    }

    public async Task<Result<IList<string>>> UpdateUserRolesAsync(Guid userId, List<string> newRoles)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result.Failure<IList<string>>(UserErrors.NotFound(userId));

        var currentRoles = await userManager.GetRolesAsync(user);

        // Add new roles that user doesn't have
        var rolesToAdd = newRoles.Except(currentRoles);
        if (rolesToAdd.Any())
        {
            var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                return Result.Failure<IList<string>>(UserErrors.RoleAssignmentFailed);
            }
        }

        // Remove roles that user has but are not selected
        var rolesToRemove = currentRoles.Except(newRoles);
        if (rolesToRemove.Any())
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                return Result.Failure<IList<string>>(UserErrors.RoleRemovalFailed);
            }
        }

        return newRoles;
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
