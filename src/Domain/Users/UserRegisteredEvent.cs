namespace CleanArch.Domain.Users;

public sealed record UserRegisteredDomainEvent(Guid UserId) : BaseEvent;
