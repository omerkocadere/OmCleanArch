using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Common.Interfaces.Authentication;

public interface ITokenProvider
{
    Task<string> CreateAsync(Guid userId);
    Task<Result<UserDto>> CreateUserWithTokensAsync(Guid userId, bool preserveCreatedAt = false);
}
