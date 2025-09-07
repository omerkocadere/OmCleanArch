using System.ComponentModel.DataAnnotations.Schema;
using CleanArch.Domain.Messages;
using CleanArch.Domain.Photos;

namespace CleanArch.Domain.Members;

public class Member : BaseAuditableEntity<Guid>
{
    public DateOnly DateOfBirth { get; set; }
    public string? ImageUrl { get; set; }
    public required string DisplayName { get; set; }
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public required string Gender { get; set; }
    public string? Description { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }

    // Navigation properties
    public List<Photo> Photos { get; set; } = [];
    public List<MemberLike> LikedByMembers { get; set; } = [];
    public List<MemberLike> LikedMembers { get; set; } = [];
    public List<Message> MessagesSent { get; set; } = [];
    public List<Message> MessagesReceived { get; set; } = [];

    // Note: Member ID now corresponds to ApplicationUser ID in Infrastructure layer
    // This maintains Clean Architecture by avoiding direct dependency on Infrastructure
}
