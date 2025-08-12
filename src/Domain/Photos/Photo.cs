using CleanArch.Domain.Members;

namespace CleanArch.Domain.Photos;

public class Photo : BaseEntity<Guid>
{
    public required string Url { get; set; }
    public string? PublicId { get; set; }

    // Navigation property
    public required Member Member { get; set; }
    public required Guid MemberId { get; set; }
}
