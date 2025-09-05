using CleanArch.Domain.Members;

namespace CleanArch.Domain.Photos;

public class Photo : BaseEntity<Guid>
{
    public required string Url { get; set; }
    public string? PublicId { get; set; }

    // Navigation property
    public Member Member { get; set; } = null!;
    public required Guid MemberId { get; set; }
}
