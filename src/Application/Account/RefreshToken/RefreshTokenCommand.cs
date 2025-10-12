using CleanArch.Application.Users.DTOs;

namespace CleanArch.Application.Account.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<UserDto>>;
