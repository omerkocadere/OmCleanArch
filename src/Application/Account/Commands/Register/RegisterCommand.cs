using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.Constants;
using CleanArch.Domain.Members;
using CleanArch.Domain.Users;
using Microsoft.AspNetCore.Identity;

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

public class RegisterCommandHandler(UserManager<User> userManager, ITokenProvider tokenProvider)
    : ICommandHandler<RegisterCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var user = command.Adapt<User>();
        var member = command.Adapt<Member>();

        user.Id = Guid.NewGuid();
        user.UserName = command.Email.ToLower();
        user.Member = member;

        user.AddDomainEvent(new UserCreatedDomainEvent(Guid.NewGuid(), user));

        var result = await userManager.CreateAsync(user, command.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToArray();
            var validationError = new ValidationError(errors);
            return Result.Failure<UserDto>(validationError);
        }

        await userManager.AddToRoleAsync(user, UserRoles.Member);

        // Create UserDto with tokens using centralized method
        return await tokenProvider.CreateUserWithTokensAsync(user);
    }
}
