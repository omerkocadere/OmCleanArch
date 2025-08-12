namespace CleanArch.Application.Members.Queries.GetMembers;

public class MemberDto
{
    public Guid Id { get; set; }
    public required string DisplayName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public required string Gender { get; set; }
    public DateTime LastActive { get; set; }
    public string? Description { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }
    public string? ImageUrl { get; set; }
}

public class PhotoDto
{
    public Guid Id { get; set; }
    public required string Url { get; set; }
    public Guid MemberId { get; set; }
}
