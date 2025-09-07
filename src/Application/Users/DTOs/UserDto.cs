namespace CleanArch.Application.Users.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? ImageUrl { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime? RefreshTokenCreatedAt { get; set; }
}
