using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CleanArch.Domain.Photos;
using CleanArch.Domain.Users;

namespace CleanArch.Domain.Members;

public class Member : BaseAuditableEntity<Guid> // Dependent Entity - ISoftDelete YOK
{
    public DateOnly DateOfBirth { get; set; }
    public string? ImageUrl { get; set; }
    public required string DisplayName { get; set; }
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public required string Gender { get; set; }
    public string? Description { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }

    // Navigation property
    [JsonIgnore]
    public List<Photo> Photos { get; set; } = [];

    [JsonIgnore]
    [ForeignKey(nameof(Id))]
    public User User { get; set; } = null!;
}
