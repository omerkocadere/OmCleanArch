using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Members;

namespace CleanArch.Application.Likes.Commands.AddLike;

public class AddLikeCommandHandler(IApplicationDbContext context, IUserContext userContext)
    : ICommandHandler<AddLikeCommand>
{
    public async Task<Result> Handle(AddLikeCommand request, CancellationToken cancellationToken)
    {
        var sourceUserId = userContext.UserId;
        if (!sourceUserId.HasValue)
            return Result.Failure(UserErrors.NotFound(Guid.Empty));

        // Get source member from userId
        var sourceMember = await context.Members.FirstOrDefaultAsync(
            m => m.Id == sourceUserId.Value,
            cancellationToken
        );

        if (sourceMember == null)
            return Result.Failure(MemberErrors.NotFound);

        // Check if target member exists
        var targetExists = await context.Members.AnyAsync(m => m.Id == request.TargetMemberId, cancellationToken);

        if (!targetExists)
            return Result.Failure(MemberErrors.NotFound);

        // Check if like already exists
        var existingLike = await context.Likes.FirstOrDefaultAsync(
            l => l.SourceMemberId == sourceMember.Id && l.TargetMemberId == request.TargetMemberId,
            cancellationToken
        );

        if (existingLike != null)
            return Result.Failure(MemberErrors.LikeAlreadyExists);

        // Don't allow self-like
        if (sourceMember.Id == request.TargetMemberId)
            return Result.Failure(MemberErrors.CannotLikeSelf);

        var like = new MemberLike { SourceMemberId = sourceMember.Id, TargetMemberId = request.TargetMemberId };

        context.Likes.Add(like);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
