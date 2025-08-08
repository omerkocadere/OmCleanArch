using CleanArch.Domain.Common;
using CleanArch.Domain.Users;

namespace CleanArch.Domain.Comments;

/// <summary>
/// Comment entity that demonstrates both audit tracking and soft delete functionality.
/// This entity uses SoftDeletableAuditableEntity base class to inherit both capabilities.
/// Perfect example for .NET 9 + EF Core 9 best practices.
/// </summary>
public sealed class Comment : FullAuditableEntity<int>
{
    public required string Content { get; set; }
    public required string AuthorName { get; set; }
    public required string Email { get; set; }
    public required int PostId { get; set; }
    public CommentStatus Status { get; set; } = CommentStatus.Pending;

    // Navigation properties
    public Guid? UserId { get; set; }
    public User? User { get; set; }
}
