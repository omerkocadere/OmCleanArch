using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Common.Interfaces.Authentication;

public interface ITokenProvider
{
    Task<string> CreateAsync(User user);
    Task<Result<UserDto>> CreateUserWithTokensAsync(User user, bool preserveCreatedAt = false);
}
