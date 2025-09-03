using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Mappings;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Members.Queries.GetMembers;

public record GetMembersQuery(PagingParams PagingParams) : IQuery<PaginatedList<MemberDto>>;

public class GetMembersQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetMembersQuery, PaginatedList<MemberDto>>
{
    public async Task<Result<PaginatedList<MemberDto>>> Handle(
        GetMembersQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = context.Members.OrderBy(x => x.DisplayName);

        return await query.ProjectToPaginatedListAsync<MemberDto>(
            request.PagingParams.PageNumber,
            request.PagingParams.PageSize
        );
    }
}
