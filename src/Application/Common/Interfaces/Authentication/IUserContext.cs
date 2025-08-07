namespace CleanArch.Application.Common.Interfaces.Authentication;

public interface IUserContext
{
    Guid? UserId { get; }
    string? UserName { get; }
}
