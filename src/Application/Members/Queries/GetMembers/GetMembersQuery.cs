using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Mappings;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Members.Queries.GetMembers;

public record GetMembersQuery(MemberParams MemberParams) : IQuery<PaginatedList<MemberDto>>;

public class GetMembersQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IQueryHandler<GetMembersQuery, PaginatedList<MemberDto>>
{
    public async Task<Result<PaginatedList<MemberDto>>> Handle(
        GetMembersQuery request,
        CancellationToken cancellationToken
    )
    {
        request.MemberParams.CurrentMemberId ??= userContext.UserId;
        var query = context.Members.AsQueryable();

        // Age filtering
        var today = DateOnly.FromDateTime(DateTime.Today);
        var minDateOfBirth = today.AddYears(-request.MemberParams.MaxAge - 1);
        var maxDateOfBirth = today.AddYears(-request.MemberParams.MinAge);

        query = query.Where(m => m.DateOfBirth >= minDateOfBirth && m.DateOfBirth <= maxDateOfBirth);

        // Gender filtering
        if (!string.IsNullOrEmpty(request.MemberParams.Gender))
        {
            query = query.Where(m => m.Gender == request.MemberParams.Gender);
        }

        // Exclude current member
        if (request.MemberParams.CurrentMemberId.HasValue)
        {
            query = query.Where(m => m.Id != request.MemberParams.CurrentMemberId.Value);
        }

        // Ordering
        query = request.MemberParams.OrderBy.ToLower() switch
        {
            "created" => query.OrderByDescending(m => m.User.Created),
            "lastactive" => query.OrderByDescending(m => m.LastActive),
            _ => query.OrderByDescending(m => m.LastActive),
        };

        return await query.ProjectToPaginatedListAsync<MemberDto>(
            request.MemberParams.PageNumber,
            request.MemberParams.PageSize
        );
    }
}
