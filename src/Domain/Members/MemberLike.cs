namespace CleanArch.Domain.Members;

public class MemberLike
{
    public required Guid SourceMemberId { get; set; }
    public Member SourceMember { get; set; } = null!;

    public required Guid TargetMemberId { get; set; }
    public Member TargetMember { get; set; } = null!;
}
