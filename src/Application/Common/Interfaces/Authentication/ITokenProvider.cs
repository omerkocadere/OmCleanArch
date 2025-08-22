using CleanArch.Domain.Users;

namespace CleanArch.Application.Common.Interfaces.Authentication;

public interface ITokenProvider
{
    string Create(User user);
}
