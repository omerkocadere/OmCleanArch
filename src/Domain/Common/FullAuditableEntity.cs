namespace CleanArch.Domain.Common;

public abstract class FullAuditableEntity<T> : BaseAuditableEntity<T>, ISoftDeletable
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

}
