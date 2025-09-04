using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Messages.Queries.Common;
using CleanArch.Domain.Common;
using CleanArch.Domain.Members;
using CleanArch.Domain.Messages;
using CleanArch.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Application.Messages.Commands.CreateMessage;

public record CreateMessageCommand(Guid RecipientId, string Content) : ICommand<MessageDto>;

public class CreateMessageCommandHandler(IApplicationDbContext context, IUserContext userContext)
    : ICommandHandler<CreateMessageCommand, MessageDto>
{
    public async Task<Result<MessageDto>> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        var senderId = userContext.UserId;
        if (!senderId.HasValue)
            return Result.Failure<MessageDto>(UserErrors.NotFound(Guid.Empty));

        // Get sender member
        var sender = await context.Members.FirstOrDefaultAsync(m => m.Id == senderId.Value, cancellationToken);

        if (sender == null)
            return Result.Failure<MessageDto>(MemberErrors.NotFound);

        // Get recipient member
        var recipient = await context.Members.FirstOrDefaultAsync(m => m.Id == request.RecipientId, cancellationToken);

        if (recipient == null)
            return Result.Failure<MessageDto>(MemberErrors.NotFound);

        // Cannot send message to yourself
        if (sender.Id == request.RecipientId)
            return Result.Failure<MessageDto>(MessageErrors.SelfMessage);

        var message = new Message
        {
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = request.Content,
            MessageSent = DateTime.UtcNow,
        };

        context.Messages.Add(message);
        await context.SaveChangesAsync(cancellationToken);

        var messageDto = new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            RecipientId = message.RecipientId,
            Content = message.Content,
            MessageSent = message.MessageSent,
            DateRead = message.DateRead,
            SenderDisplayName = sender.DisplayName,
            SenderImageUrl = sender.ImageUrl ?? string.Empty,
            RecipientDisplayName = recipient.DisplayName,
            RecipientImageUrl = recipient.ImageUrl ?? string.Empty,
        };

        return Result.Success(messageDto);
    }
}
