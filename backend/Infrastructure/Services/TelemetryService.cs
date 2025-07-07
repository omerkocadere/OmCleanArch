using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Users.DTOs;
using CleanArch.Infrastructure.OpenTelemetry;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.Services;

public class TelemetryService(ILogger<TelemetryService> logger) : ITelemetryService
{
    public void RecordUserCreated(UserDto user)
    {
        DiagnosticsConfig.CreatedUserCounter.Add(
            1,
            new KeyValuePair<string, object?>("user.name", user.FirstName),
            new KeyValuePair<string, object?>("user.id", user.Id),
            new KeyValuePair<string, object?>("user.lastName", user.LastName)
        );

        logger.LogInformation(
            "Telemetry recorded for user creation: {UserId} - {FirstName} {LastName}",
            user.Id,
            user.FirstName,
            user.LastName
        );
    }
}
