namespace CleanArch.Domain.Common;

public abstract class BaseAuditableEntity<T> : BaseEntity<T>, IAuditableEntity
    where T : IEquatable<T>
{
    public DateTimeOffset Created { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }
}
