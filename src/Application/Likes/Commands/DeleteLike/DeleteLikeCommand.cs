using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;
using CleanArch.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Application.Likes.Commands.DeleteLike;

public record DeleteLikeCommand(Guid TargetMemberId) : ICommand;

public class DeleteLikeCommandHandler(IApplicationDbContext context, IUserContext userContext)
    : ICommandHandler<DeleteLikeCommand>
{
    public async Task<Result> Handle(DeleteLikeCommand request, CancellationToken cancellationToken)
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

        var like = await context.Likes.FirstOrDefaultAsync(
            l => l.SourceMemberId == sourceMember.Id && l.TargetMemberId == request.TargetMemberId,
            cancellationToken
        );

        if (like == null)
            return Result.Failure(MemberErrors.LikeNotFound);

        context.Likes.Remove(like);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
