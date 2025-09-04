using CleanArch.Domain.Members;

namespace CleanArch.Domain.Messages;

public class Message : BaseEntity<Guid>
{
    public required string Content { get; set; }
    public DateTime? DateRead { get; set; }
    public DateTime MessageSent { get; set; } = DateTime.UtcNow;
    public bool SenderDeleted { get; set; }
    public bool RecipientDeleted { get; set; }

    // nav properties
    public required Guid SenderId { get; set; }
    public Member Sender { get; set; } = null!;
    public required Guid RecipientId { get; set; }
    public Member Recipient { get; set; } = null!;
}
