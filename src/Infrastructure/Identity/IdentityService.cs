using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Identity;

internal sealed class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<Result<UserDto>> Login(string password, string email, string refreshToken)
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
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(3);
        user.RefreshTokenCreatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);

        return result.Succeeded ? user.Adapt<UserDto>() : Result.Failure<UserDto>(ConvertIdentityErrors(result.Errors));
    }

    public async Task<Result<UserDto>> CreateUserAsync(
        string userName,
        string email,
        string password,
        string refreshToken,
        string? displayName = null,
        string? firstName = null,
        string? lastName = null,
        string? imageUrl = null,
        IEnumerable<string> roles = null!
    )
    {
        // Create ApplicationUser entity
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            DisplayName = displayName ?? userName,
            FirstName = firstName,
            LastName = lastName,
            ImageUrl = imageUrl,
            RefreshToken = refreshToken,
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(3),
            RefreshTokenCreatedAt = DateTime.UtcNow,
        };

        // Create User with Identity
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return Result.Failure<UserDto>(ConvertIdentityErrors(result.Errors));
        }

        result = await userManager.AddToRolesAsync(user, roles);
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

    public async Task<Result> UpdateRefreshTokenAsync(Guid userId, DateTime expiry, string? refreshToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result.Failure(UserErrors.NotFound(userId));

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = expiry;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded ? Result.Success() : Result.Failure(ConvertIdentityErrors(result.Errors));
    }

    public async Task<Result<UserDto>> RefreshToken(string refreshToken)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user is null)
        {
            return Result.Failure<UserDto>(AuthenticationErrors.InvalidRefreshToken);
        }

        user.RefreshToken = Guid.NewGuid().ToString();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(3);
        user.RefreshTokenCreatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded ? user.Adapt<UserDto>() : Result.Failure<UserDto>(ConvertIdentityErrors(result.Errors));
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user != null && await userManager.CheckPasswordAsync(user, password);
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

    public async Task<bool> HasAnyUsersAsync()
    {
        return await userManager.Users.AnyAsync();
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

        if (displayName != null)
            user.DisplayName = displayName;
        if (firstName != null)
            user.FirstName = firstName;
        if (lastName != null)
            user.LastName = lastName;
        if (imageUrl != null)
            user.ImageUrl = imageUrl;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded ? Result.Success() : Result.Failure(ConvertIdentityErrors(result.Errors));
    }

    public async Task<IList<string>> GetUserRolesAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user != null ? await userManager.GetRolesAsync(user) : [];
    }

    public async Task<Result> AddToRolesAsync(Guid userId, IEnumerable<string> roles)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result.Failure(UserErrors.NotFound(userId));

        var result = await userManager.AddToRolesAsync(user, roles);
        return result.Succeeded ? Result.Success() : Result.Failure(ConvertIdentityErrors(result.Errors));
    }

    public async Task<Result<IList<string>>> UpdateUserRolesAsync(Guid userId, IEnumerable<string> newRoles)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result.Failure<IList<string>>(UserErrors.NotFound(userId));

        var selectedRoles = newRoles.ToArray();
        var currentRoles = await userManager.GetRolesAsync(user);

        // Add new roles that user doesn't have
        var rolesToAdd = selectedRoles.Except(currentRoles);
        if (rolesToAdd.Any())
        {
            var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                return Result.Failure<IList<string>>(UserErrors.RoleAssignmentFailed);
            }
        }

        // Remove roles that user has but are not selected
        var rolesToRemove = currentRoles.Except(selectedRoles);
        if (rolesToRemove.Any())
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                return Result.Failure<IList<string>>(UserErrors.RoleRemovalFailed);
            }
        }

        // Get the final roles after all operations
        var finalRoles = await userManager.GetRolesAsync(user);
        return Result.Success(finalRoles);
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
