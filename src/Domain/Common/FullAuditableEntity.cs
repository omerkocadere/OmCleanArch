namespace CleanArch.Domain.Common;

public abstract class FullAuditableEntity<T> : BaseAuditableEntity<T>, ISoftDeletable
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public virtual void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;

        // Update audit information as well
        LastModified = DateTimeOffset.UtcNow;
        LastModifiedBy = deletedBy;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;

        // Update audit information as well
        LastModified = DateTimeOffset.UtcNow;
        // LastModifiedBy will be set by AuditableEntityInterceptor
    }
}
