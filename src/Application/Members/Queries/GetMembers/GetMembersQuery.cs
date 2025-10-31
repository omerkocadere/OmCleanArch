using CleanArch.Application.Common.Extensions;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;

namespace CleanArch.Application.Members.Queries.GetMembers;

public record GetMembersQuery(MemberParams MemberParams) : QueryParamsBase, IQuery<PaginatedList<MemberDto>>;

public class GetMembersQueryHandler(IApplicationDbContext context, ICurrentUser userContext)
    : IQueryHandler<GetMembersQuery, PaginatedList<MemberDto>>
{
    public async Task<Result<PaginatedList<MemberDto>>> Handle(
        GetMembersQuery request,
        CancellationToken cancellationToken
    )
    {
        var currentMemberId = request.MemberParams.CurrentMemberId ?? userContext.UserId;
        var query = context.Members.AsQueryable();

        // Age filtering using computed properties with defaults
        var today = DateOnly.FromDateTime(DateTime.Today);
        var minDateOfBirth = today.AddYears(-request.MemberParams.MaxAgeValue - 1);
        var maxDateOfBirth = today.AddYears(-request.MemberParams.MinAgeValue);

        query = query.Where(m => m.DateOfBirth >= minDateOfBirth && m.DateOfBirth <= maxDateOfBirth);

        // Gender filtering
        if (!string.IsNullOrEmpty(request.MemberParams.Gender))
        {
            query = query.Where(m => m.Gender == request.MemberParams.Gender);
        }

        // Exclude current member
        if (currentMemberId.HasValue)
        {
            query = query.Where(m => m.Id != currentMemberId.Value);
        }

        // Ordering using computed property with default
        query = request.MemberParams.OrderByValue.ToLower() switch
        {
            "created" => query.OrderByDescending(m => m.Created),
            "lastactive" => query.OrderByDescending(m => m.LastActive),
            _ => query.OrderByDescending(m => m.LastActive),
        };

        return await query.ProjectToPagedAsync<MemberDto>(request);
    }
}
