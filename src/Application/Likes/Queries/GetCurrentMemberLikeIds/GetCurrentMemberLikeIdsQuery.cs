using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;
using CleanArch.Domain.Members;

namespace CleanArch.Application.Likes.Queries.GetCurrentMemberLikeIds;

public record GetCurrentMemberLikeIdsQuery(Guid? MemberId = null) : IQuery<IReadOnlyList<Guid>>;

public class GetCurrentMemberLikeIdsQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IQueryHandler<GetCurrentMemberLikeIdsQuery, IReadOnlyList<Guid>>
{
    public async Task<Result<IReadOnlyList<Guid>>> Handle(
        GetCurrentMemberLikeIdsQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = request.MemberId ?? userContext.UserId;

        if (!userId.HasValue)
            return Result.Failure<IReadOnlyList<Guid>>(UserErrors.NotFound(Guid.Empty));

        // Get member from userId
        var member = await context.Members.FirstOrDefaultAsync(m => m.Id == userId.Value, cancellationToken);

        if (member == null)
            return Result.Failure<IReadOnlyList<Guid>>(MemberErrors.NotFound);

        var likedMemberIds = await context
            .Likes.Where(x => x.SourceMemberId == member.Id)
            .Select(x => x.TargetMemberId)
            .ToListAsync(cancellationToken);

        return Result.Success((IReadOnlyList<Guid>)likedMemberIds);
    }
}
