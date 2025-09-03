using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.Members;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Users.Create;

public sealed record CreateUserCommand : ICommand<UserDto>
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

public class CreateUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider
) : ICommandHandler<CreateUserCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(command.Email);

        if (emailResult.IsFailure)
        {
            return Result.Failure<UserDto>(emailResult.Error);
        }

        var emailExists = await context.Users.AnyAsync(u => u.Email == emailResult.Value, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<UserDto>(UserErrors.EmailNotUnique);
        }

        var user = command.Adapt<User>();
        var member = command.Adapt<Member>();

        user.Id = Guid.NewGuid();
        user.Email = emailResult.Value;
        user.PasswordHash = passwordHasher.Hash(command.Password);
        user.Member = member;

        // Set the same ID for one-to-one relationship
        // member.Id = user.Id;
        // member.User = user;

        user.AddDomainEvent(new UserCreatedDomainEvent(Guid.NewGuid(), user));

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        var userDto = user.Adapt<UserDto>();
        userDto.Token = tokenProvider.Create(user);

        return userDto;
    }
}
