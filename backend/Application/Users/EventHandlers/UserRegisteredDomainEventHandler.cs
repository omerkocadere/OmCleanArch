using CleanArch.Domain.Users;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Users.EventHandlers;

public class UserRegisteredDomainEventHandler(ILogger<UserRegisteredDomainEventHandler> logger)
    : INotificationHandler<UserRegisteredDomainEvent>
{
    public Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User registered event handled for user {UserId}",
            notification.User.Id
        );

        // Here you could add business logic like:
        // - Send welcome email
        // - Create user profile
        // - Set up default settings
        // - Trigger other business processes

        return Task.CompletedTask;
    }
}
