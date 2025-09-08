namespace CleanArch.Application.Users.Models;

public sealed class CreateUserRequest
{
    public required string UserName { get; init; }

    public required string Email { get; init; }

    public required string Password { get; init; }

    public required DateTime RefreshTokenExpiry { get; init; }

    public required string RefreshToken { get; init; }

    public string? DisplayName { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string? ImageUrl { get; init; }

    public IEnumerable<string> Roles { get; init; } = [];
}
