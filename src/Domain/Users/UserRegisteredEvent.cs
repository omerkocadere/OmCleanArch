namespace CleanArch.Domain.Users;

public sealed record UserRegisteredDomainEvent(User User) : BaseEvent;
