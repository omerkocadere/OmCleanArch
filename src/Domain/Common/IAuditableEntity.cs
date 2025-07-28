namespace CleanArch.Domain.Common;

public interface IAuditableEntity
{
    DateTime Created { get; set; }
    Guid? CreatedBy { get; set; }
    DateTime LastModified { get; set; }
    Guid? LastModifiedBy { get; set; }
}
