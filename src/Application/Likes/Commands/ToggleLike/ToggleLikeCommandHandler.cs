using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Members;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Likes.Commands.ToggleLike;

public class ToggleLikeCommandHandler(IApplicationDbContext context, ICurrentUser userContext)
    : ICommandHandler<ToggleLikeCommand>
{
    public async Task<Result> Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
    {
        var sourceUserId = userContext.UserId;
        if (!sourceUserId.HasValue)
            return Result.Failure(UserErrors.NotFound(Guid.Empty));

        // Start atomic transaction for consistency
        using var transaction = await context.BeginTransactionAsync(cancellationToken);

        // Get source member from userId - Member.Id should equal User.Id
        var sourceMember = await context.Members.FirstOrDefaultAsync(
            m => m.Id == sourceUserId.Value,
            cancellationToken
        );

        if (sourceMember == null)
            return Result.Failure(MemberErrors.NotFound);

        // Don't allow self-like
        if (sourceMember.Id == request.TargetMemberId)
            return Result.Failure(MemberErrors.CannotLikeSelf);

        // Check if target member exists
        var targetExists = await context.Members.AnyAsync(m => m.Id == request.TargetMemberId, cancellationToken);

        if (!targetExists)
            return Result.Failure(MemberErrors.NotFound);

        // Check if like already exists
        var existingLike = await context.Likes.FirstOrDefaultAsync(
            l => l.SourceMemberId == sourceMember.Id && l.TargetMemberId == request.TargetMemberId,
            cancellationToken
        );

        if (existingLike == null)
        {
            // Add new like
            var like = new MemberLike { SourceMemberId = sourceMember.Id, TargetMemberId = request.TargetMemberId };
            context.Likes.Add(like);
        }
        else
        {
            // Remove existing like
            context.Likes.Remove(existingLike);
        }

        await context.SaveChangesAsync(cancellationToken);

        // Commit transaction - operation successful
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
