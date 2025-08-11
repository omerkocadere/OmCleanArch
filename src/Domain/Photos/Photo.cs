using System.Text.Json.Serialization;
using CleanArch.Domain.Members;

namespace CleanArch.Domain.Photos;

public class Photo : BaseEntity<Guid>
{
    public required string Url { get; set; }
    public string? PublicId { get; set; }

    // Navigation property
    [JsonIgnore]
    public required Member Member { get; set; }
    public required Guid MemberId { get; set; }
}
