using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Members.Queries.GetMembers;
using CleanArch.Domain.Common;
using CleanArch.Domain.Members;

namespace CleanArch.Application.Members.Queries.GetMember;

public record GetMemberQuery(Guid Id) : IQuery<MemberDto?>;

public class GetMemberQueryHandler(IApplicationDbContext context) : IQueryHandler<GetMemberQuery, MemberDto?>
{
    public async Task<Result<MemberDto?>> Handle(GetMemberQuery request, CancellationToken cancellationToken)
    {
        var member = await context
            .Members.Where(m => m.Id == request.Id)
            .ProjectToType<MemberDto>()
            .FirstOrDefaultAsync(cancellationToken);

        return member ?? Result.Failure<MemberDto?>(MemberErrors.NotFound);
    }
}
