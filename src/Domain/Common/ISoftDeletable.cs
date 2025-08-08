namespace CleanArch.Domain.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }

    void SoftDelete(Guid? deletedBy = null);
    void Restore();
}
