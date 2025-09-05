using CleanArch.Application.Common.Models;

namespace CleanArch.Application.Members.Queries.GetMembers;

public class MemberParams : PagingParams
{
    public string? Gender { get; set; }
    public Guid? CurrentMemberId { get; set; }

    // Nullable for binding, computed properties for business logic
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public string? OrderBy { get; set; }

    // Computed properties with defaults
    public int MinAgeValue => MinAge ?? 18;
    public int MaxAgeValue => MaxAge ?? 100;
    public string OrderByValue => OrderBy ?? "lastActive";
}
