namespace CleanArch.Domain.Common;

public abstract class BaseAuditableEntity<T> : BaseEntity<T>, IAuditableEntity
    where T : IEquatable<T>
{
    public DateTime Created { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }
}
