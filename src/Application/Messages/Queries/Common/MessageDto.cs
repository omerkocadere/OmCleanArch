namespace CleanArch.Application.Messages.Queries.Common;

public record MessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime MessageSent { get; set; }
    public DateTime? DateRead { get; set; }
    public string SenderDisplayName { get; set; } = string.Empty;
    public string SenderImageUrl { get; set; } = string.Empty;
    public string RecipientDisplayName { get; set; } = string.Empty;
    public string RecipientImageUrl { get; set; } = string.Empty;
}
