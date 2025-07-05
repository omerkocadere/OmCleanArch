using CleanArch.Domain.Users;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Users.EventHandlers;

public class UserRegisteredDomainEventHandler(ILogger<UserRegisteredDomainEventHandler> logger)
    : INotificationHandler<UserRegisteredDomainEvent>
{
    public Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User registered event handled for user {FirstName} {LastName} and email {Email}",
            notification.User.FirstName,
            notification.User.LastName,
            notification.User.Email
        );

        // Here you could add business logic like:
        // - Send welcome email
        // - Create user profile
        // - Set up default settings
        // - Trigger other business processes

        return Task.CompletedTask;
    }
}
