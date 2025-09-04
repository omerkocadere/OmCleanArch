using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;
using CleanArch.Domain.Members;
using CleanArch.Domain.Messages;
using CleanArch.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Application.Messages.Commands.DeleteMessage;

public record DeleteMessageCommand(Guid MessageId) : ICommand;

public class DeleteMessageCommandHandler(IApplicationDbContext context, IUserContext userContext)
    : ICommandHandler<DeleteMessageCommand>
{
    public async Task<Result> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        if (!userId.HasValue)
            return Result.Failure(UserErrors.NotFound(Guid.Empty));

        // Get member
        var member = await context.Members.FirstOrDefaultAsync(m => m.Id == userId.Value, cancellationToken);

        if (member == null)
            return Result.Failure(MemberErrors.NotFound);

        var message = await context.Messages.FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null)
            return Result.Failure(MessageErrors.NotFoundGeneral);

        // Check if user can delete this message
        if (message.SenderId != member.Id && message.RecipientId != member.Id)
            return Result.Failure(MessageErrors.CannotDelete);

        // Mark as deleted for the appropriate user
        if (message.SenderId == member.Id)
            message.SenderDeleted = true;
        if (message.RecipientId == member.Id)
            message.RecipientDeleted = true;

        // If both users deleted, remove from database
        if (message.SenderDeleted && message.RecipientDeleted)
        {
            context.Messages.Remove(message);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
