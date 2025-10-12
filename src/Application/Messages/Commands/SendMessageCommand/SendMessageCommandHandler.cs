using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Messages.Queries.Common;
using CleanArch.Domain.Members;
using CleanArch.Domain.Messages;

namespace CleanArch.Application.Messages.Commands.SendMessageCommand;

public class SendMessageCommandHandler(IApplicationDbContext context, IUserContext userContext)
    : ICommandHandler<SendMessageCommand, MessageDto>
{
    public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var senderId = userContext.UserId;
        if (!senderId.HasValue)
            return Result.Failure<MessageDto>(UserErrors.NotFound(Guid.Empty));

        // Get sender and recipient in one query
        var members = await context
            .Members.Where(m => m.Id == senderId.Value || m.Id == request.RecipientId)
            .ToListAsync(cancellationToken);

        var sender = members.FirstOrDefault(m => m.Id == senderId.Value);
        var recipient = members.FirstOrDefault(m => m.Id == request.RecipientId);

        if (sender == null)
            return Result.Failure<MessageDto>(MemberErrors.NotFound);

        if (recipient == null)
            return Result.Failure<MessageDto>(MemberErrors.NotFound);

        if (sender.Id == request.RecipientId)
            return Result.Failure<MessageDto>(MessageErrors.SelfMessage);

        // Check if recipient is in the group
        var group = await context
            .Groups.Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Name == request.GroupName, cancellationToken);

        var userInGroup = group?.Connections?.Any(x => x.UserId == request.RecipientId) ?? false;

        // Create message
        var message = new Message
        {
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = request.Content,
            MessageSent = DateTime.UtcNow,
            DateRead = userInGroup ? DateTime.UtcNow : null, // Mark as read if user is in group
            Sender = sender,
            Recipient = recipient,
        };

        context.Messages.Add(message);
        await context.SaveChangesAsync(cancellationToken);

        // Convert to DTO using Mapster (follows project convention)
        var messageDto = message.Adapt<MessageDto>();

        return Result.Success(messageDto);
    }
}
