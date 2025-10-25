namespace CleanArch.Application.Common.Interfaces.Authentication;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    string? FirstName { get; }
    string? LastName { get; }
}
