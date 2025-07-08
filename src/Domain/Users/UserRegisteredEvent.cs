namespace CleanArch.Domain.Users;

public sealed record UserCreatedDomainEvent(User User) : BaseEvent;
