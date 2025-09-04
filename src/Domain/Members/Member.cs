using System.ComponentModel.DataAnnotations.Schema;
using CleanArch.Domain.Messages;
using CleanArch.Domain.Photos;
using CleanArch.Domain.Users;

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

    // Navigation property
    public List<Photo> Photos { get; set; } = [];
    public List<MemberLike> LikedByMembers { get; set; } = [];
    public List<MemberLike> LikedMembers { get; set; } = [];
    public List<Message> MessagesSent { get; set; } = [];
    public List<Message> MessagesReceived { get; set; } = [];

    [ForeignKey(nameof(Id))]
    public required User User { get; set; }
}
