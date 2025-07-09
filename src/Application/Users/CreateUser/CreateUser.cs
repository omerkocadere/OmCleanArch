using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Users.CreateUser;

public sealed record CreateUserCommand : ICommand<UserDto>
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Password { get; set; }
}

public class CreateUser(IApplicationDbContext context, IMapper mapper)
    : ICommandHandler<CreateUserCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken
    )
    {
        var user = mapper.Map<User>(request);
        user.PasswordHash = HashPassword(request.Password);

        user.AddDomainEvent(new UserCreatedDomainEvent(user));

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return mapper.Map<UserDto>(user);
    }

    private static string HashPassword(string password)
    {
        // TODO: Replace with a real password hashing implementation
        return "hashed_" + password;
    }
}
