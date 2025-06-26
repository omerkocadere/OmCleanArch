namespace CleanArch.Application.Users.GetByEmail;

public sealed record UserResponse
{
    public int Id { get; init; }

    public required string Email { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }
}
