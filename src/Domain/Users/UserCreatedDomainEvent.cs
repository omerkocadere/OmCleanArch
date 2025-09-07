namespace CleanArch.Domain.Users;

/// <summary>
/// Domain event raised when a new user is created.
/// This triggers business logic such as sending welcome emails, recording telemetry, etc.
/// </summary>
public sealed record UserCreatedDomainEvent(Guid Id, Guid UserId, string UserName, string Email) : BaseEvent(Id);
