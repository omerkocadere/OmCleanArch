using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Mappings;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Members.Queries.GetMembers;
using CleanArch.Domain.Common;
using CleanArch.Domain.Members;
using CleanArch.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Application.Likes.Queries.GetMemberLikes;

public record GetMemberLikesQuery(LikesParams LikesParams) : IQuery<PaginatedList<MemberDto>>;

public class GetMemberLikesQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IQueryHandler<GetMemberLikesQuery, PaginatedList<MemberDto>>
{
    public async Task<Result<PaginatedList<MemberDto>>> Handle(
        GetMemberLikesQuery request,
        CancellationToken cancellationToken
    )
    {
        var requestUserId = request.LikesParams.MemberId ?? userContext.UserId;

        if (!requestUserId.HasValue)
            return Result.Failure<PaginatedList<MemberDto>>(UserErrors.NotFound(Guid.Empty));

        // Get member from userId
        var requestMember = await context.Members.FirstOrDefaultAsync(
            m => m.Id == requestUserId.Value,
            cancellationToken
        );

        if (requestMember == null)
            return Result.Failure<PaginatedList<MemberDto>>(MemberErrors.NotFound);

        var query = context.Likes.AsQueryable();
        IQueryable<Member> result;

        switch (request.LikesParams.Predicate.ToLower())
        {
            case "liked":
                // Members that current user has liked
                result = query.Where(like => like.SourceMemberId == requestMember.Id).Select(like => like.TargetMember);
                break;

            case "likedby":
                // Members that have liked the current user
                result = query.Where(like => like.TargetMemberId == requestMember.Id).Select(like => like.SourceMember);
                break;

            default:
                // Mutual likes: people who liked current user AND current user also liked them
                var currentUserLikedIds = await context
                    .Likes.Where(x => x.SourceMemberId == requestMember.Id)
                    .Select(x => x.TargetMemberId)
                    .ToListAsync(cancellationToken);

                result = query
                    .Where(x => x.TargetMemberId == requestMember.Id && currentUserLikedIds.Contains(x.SourceMemberId))
                    .Select(x => x.SourceMember);
                break;
        }

        return await result.ProjectToPaginatedListAsync<MemberDto>(
            request.LikesParams.PageNumberValue,
            request.LikesParams.PageSizeValue
        );
    }
}
