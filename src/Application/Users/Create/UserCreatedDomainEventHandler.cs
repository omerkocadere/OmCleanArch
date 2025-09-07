using CleanArch.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Users.Create;

/// <summary>
/// Handles UserCreatedDomainEvent to perform business logic after user creation
/// such as sending welcome emails, recording telemetry, etc.
/// </summary>
public sealed class UserCreatedDomainEventHandler : INotificationHandler<UserCreatedDomainEvent>
{
    private readonly ILogger<UserCreatedDomainEventHandler> _logger;

    public UserCreatedDomainEventHandler(ILogger<UserCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "User created successfully: {UserId} - {UserName}",
            notification.UserId,
            notification.UserName
        );

        // TODO: Add business logic here
        // - Send welcome email to {Email}
        // - Record telemetry for user {UserId}
        // - Initialize user preferences
        // - Trigger other domain events

        await Task.CompletedTask;
    }
}
