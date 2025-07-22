using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Users.Create;

public sealed record CreateUserCommand : ICommand<UserDto>
{
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Password { get; set; }
}

public class CreateUser(IApplicationDbContext context, IPasswordHasher passwordHasher, IMapper mapper)
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

        var user = mapper.Map<User>(command);
        user.Id = Guid.NewGuid();
        user.PasswordHash = passwordHasher.Hash(command.Password);

        user.AddDomainEvent(new UserCreatedDomainEvent(Guid.NewGuid(), user));

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return mapper.Map<UserDto>(user);
    }
}
