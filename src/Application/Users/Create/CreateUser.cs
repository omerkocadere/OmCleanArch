using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Users.Create;

public sealed record CreateUserCommand : ICommand<Guid>
{
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Password { get; set; }
}

public class CreateUser(IApplicationDbContext context, IPasswordHasher passwordHasher, IMapper mapper)
    : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var emailExists = await context.Users.AnyAsync(
            u => u.Email.Equals(request.Email, StringComparison.CurrentCultureIgnoreCase),
            cancellationToken
        );

        if (emailExists)
        {
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);
        }

        var user = mapper.Map<User>(request);
        user.Id = Guid.NewGuid();
        user.PasswordHash = passwordHasher.Hash(request.Password);

        user.AddDomainEvent(new UserCreatedDomainEvent(Guid.NewGuid(), user));

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
