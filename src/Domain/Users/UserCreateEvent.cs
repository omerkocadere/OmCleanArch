namespace CleanArch.Domain.Users;

public sealed record UserCreatedDomainEvent(Guid Id, User User) : BaseEvent(Id);
