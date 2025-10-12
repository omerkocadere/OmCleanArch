using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Users.DTOs;
using CleanArch.Application.Users.Models;
using CleanArch.Domain.Constants;
using CleanArch.Domain.Members;

namespace CleanArch.Application.Account.Register;

public class RegisterCommandHandler(
    IIdentityService identityService,
    IApplicationDbContext context,
    ITokenProvider tokenProvider
) : ICommandHandler<RegisterCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        string token = tokenProvider.GenerateRefreshToken();
        DateTime expiry = DateTime.UtcNow.AddDays(3);

        // Start database transaction for atomicity
        using var transaction = await context.BeginTransactionAsync(cancellationToken);

        // Create the user with all details in one call
        var createUserRequest = new CreateUserRequest
        {
            UserName = command.Email.ToLower(),
            Email = command.Email,
            Password = command.Password,
            RefreshTokenExpiry = expiry,
            RefreshToken = token,
            DisplayName = command.DisplayName,
            FirstName = command.FirstName,
            LastName = command.LastName,
            ImageUrl = null,
            Roles = [UserRoles.Member],
        };

        var result = await identityService.CreateUserAsync(createUserRequest);

        if (result.IsFailure)
        {
            return Result.Failure<UserDto>(result.Error);
        }

        var member = command.Adapt<Member>();
        member.Id = result.Value.Id;
        member.LastActive = DateTime.UtcNow;

        context.Members.Add(member);
        await context.SaveChangesAsync(cancellationToken);

        // Commit transaction - all operations successful
        await transaction.CommitAsync(cancellationToken);

        result.Value.Token = await tokenProvider.CreateAsync(result.Value.Id);
        return result.Value;
    }
}
