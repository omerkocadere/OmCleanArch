namespace CleanArch.Application.Admin.DTOs;

public class UserWithRolesDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public IList<string> Roles { get; set; } = [];
}
