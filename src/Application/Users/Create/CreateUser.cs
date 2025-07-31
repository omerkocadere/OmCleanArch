using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Users.Create;

public sealed record CreateUserCommand : ICommand<UserDto>
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CreateUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    : ICommandHandler<CreateUserCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var emailExists = await context.Users.AnyAsync(
            u => u.Email.ToLower() == command.Email.ToLower(),
            cancellationToken
        );

        if (emailExists)
        {
            return Result.Failure<UserDto>(UserErrors.EmailNotUnique);
        }

        var user = command.Adapt<User>();
        user.Id = Guid.NewGuid();
        user.PasswordHash = passwordHasher.Hash(command.Password);

        user.AddDomainEvent(new UserCreatedDomainEvent(Guid.NewGuid(), user));

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return user.Adapt<UserDto>();
    }
}
