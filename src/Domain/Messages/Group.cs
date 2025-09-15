namespace CleanArch.Domain.Messages;

public class Group(string name) : BaseEntity<Guid>
{
    public string Name { get; set; } = name;
    public ICollection<Connection> Connections { get; set; } = [];
}
