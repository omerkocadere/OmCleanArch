using CleanArch.Domain.Users;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Users.Create;

public class UserCreatedWelcomeHandler(ILogger<UserCreatedWelcomeHandler> logger)
    : INotificationHandler<UserCreatedDomainEvent>
{
    public Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var user = notification.User;

        // Simulate sending a welcome email
        logger.LogInformation("Welcome email sent to user {UserId} ({Email})", user.Id, user.Email);

        // Simulate initializing user profile
        logger.LogInformation("Initialized profile for user {UserId}", user.Id);

        return Task.CompletedTask;
    }
}
