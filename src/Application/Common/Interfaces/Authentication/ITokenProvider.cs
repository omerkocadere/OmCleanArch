using CleanArch.Domain.Users;

namespace CleanArch.Application.Common.Interfaces.Authentication;

public interface ITokenProvider
{
    Task<string> CreateAsync(User user);
}
