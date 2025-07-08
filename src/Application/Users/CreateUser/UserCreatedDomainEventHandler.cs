using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Users;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Users.EventHandlers;

public class UserCreatedDomainEventHandler(
    ILogger<UserCreatedDomainEventHandler> logger,
    ITelemetryService telemetryService,
    IMapper mapper
) : INotificationHandler<UserCreatedDomainEvent>
{
    public Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var user = notification.User;

        logger.LogInformation(
            "User registered event handled for user {FirstName} {LastName} and email {Email}",
            user.FirstName,
            user.LastName,
            user.Email
        );

        // Record telemetry
        var userDto = mapper.Map<UserDto>(user);
        telemetryService.RecordUserCreated(userDto);

        // Here you could add business logic like:
        // - Send welcome email
        // - Create user profile
        // - Set up default settings
        // - Trigger other business processes

        return Task.CompletedTask;
    }
}
