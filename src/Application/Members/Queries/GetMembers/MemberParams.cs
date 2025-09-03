using CleanArch.Application.Common.Models;

namespace CleanArch.Application.Members.Queries.GetMembers;

public class MemberParams : PagingParams
{
    public string? Gender { get; set; }
    public Guid? CurrentMemberId { get; set; }
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 100;
    public string OrderBy { get; set; } = "lastActive";
}
