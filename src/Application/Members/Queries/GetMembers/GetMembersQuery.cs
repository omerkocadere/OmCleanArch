using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Mappings;
using CleanArch.Application.Common.Models;

namespace CleanArch.Application.Members.Queries.GetMembers;

public record GetMembersQuery : IQuery<List<MemberDto>>;

public class GetMembersQueryHandler(IApplicationDbContext context) : IQueryHandler<GetMembersQuery, List<MemberDto>>
{
    public async Task<Result<List<MemberDto>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        return await context.Members.ProjectToListAsync<MemberDto>();
    }
}
