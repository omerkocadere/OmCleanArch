using CleanArch.Application.Users.DTOs;

namespace CleanArch.Application.Account.Login;

public sealed record LoginCommand : ICommand<UserDto>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
