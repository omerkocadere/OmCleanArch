using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.Constants;
using CleanArch.Domain.Members;

namespace CleanArch.Application.Account.Commands.Register;

public sealed record RegisterCommand : ICommand<UserDto>
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
}

public class RegisterCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    : ICommandHandler<RegisterCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        // Start database transaction for atomicity
        using var transaction = await context.BeginTransactionAsync(cancellationToken);

        // Create the user with all details in one call
        var (result, userDto) = await identityService.CreateUserAsync(
            command.Email.ToLower(),
            command.Email,
            command.Password,
            command.DisplayName,
            command.FirstName,
            command.LastName
        );

        if (!result.IsSuccess || userDto is null)
        {
            return Result.Failure<UserDto>(result.Error);
        }

        // Add user to Member role
        var addRoleResult = await identityService.AddToRolesAsync(userDto.Id, [UserRoles.Member]);
        if (!addRoleResult.IsSuccess)
        {
            return Result.Failure<UserDto>(addRoleResult.Error);
        }

        // Create Member domain entity
        var member = new Member
        {
            Id = userDto.Id,
            DateOfBirth = command.DateOfBirth,
            DisplayName = command.DisplayName,
            Gender = command.Gender,
            City = command.City,
            Country = command.Country,
            LastActive = DateTime.UtcNow,
            Description = string.Empty, // Default empty description
        };

        context.Members.Add(member);
        await context.SaveChangesAsync(cancellationToken);

        // Commit transaction - all operations successful
        await transaction.CommitAsync(cancellationToken);

        return Result.Success(userDto);
    }
}
