namespace CleanArch.Domain.Messages;

public class Connection(string connectionId, Guid userId) : BaseEntity<Guid>
{
    public string ConnectionId { get; set; } = connectionId;
    public Guid UserId { get; set; } = userId;
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
}
