using CleanArch.Application.Users.DTOs;

namespace CleanArch.Application.Common.Interfaces;

public interface ITelemetryService
{
    void RecordUserCreated(UserDto user);
}
