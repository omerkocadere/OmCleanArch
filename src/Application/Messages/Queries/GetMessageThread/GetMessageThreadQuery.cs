using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Messages.Queries.Common;
using CleanArch.Domain.Common;
using CleanArch.Domain.Members;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Messages.Queries.GetMessageThread;

public record GetMessageThreadQuery(Guid RecipientId) : IQuery<IReadOnlyList<MessageDto>>;

public class GetMessageThreadQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IQueryHandler<GetMessageThreadQuery, IReadOnlyList<MessageDto>>
{
    public async Task<Result<IReadOnlyList<MessageDto>>> Handle(
        GetMessageThreadQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = userContext.UserId;
        if (!userId.HasValue)
            return Result.Failure<IReadOnlyList<MessageDto>>(UserErrors.NotFound(Guid.Empty));

        // Get member
        var member = await context.Members.FirstOrDefaultAsync(m => m.Id == userId.Value, cancellationToken);

        if (member == null)
            return Result.Failure<IReadOnlyList<MessageDto>>(MemberErrors.NotFound);

        // Mark unread messages as read
        await context
            .Messages.Where(x => x.RecipientId == member.Id && x.SenderId == request.RecipientId && x.DateRead == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.DateRead, DateTime.UtcNow), cancellationToken);

        // Get message thread
        var messages = await context
            .Messages.Where(x =>
                (x.RecipientId == member.Id && x.SenderId == request.RecipientId && !x.RecipientDeleted)
                || (x.SenderId == member.Id && x.RecipientId == request.RecipientId && !x.SenderDeleted)
            )
            .OrderBy(x => x.MessageSent)
            .ProjectToType<MessageDto>()
            .ToListAsync(cancellationToken);

        return Result.Success((IReadOnlyList<MessageDto>)messages);
    }
}
