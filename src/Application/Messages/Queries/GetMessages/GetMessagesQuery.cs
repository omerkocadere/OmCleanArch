using CleanArch.Application.Common.Extensions;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Messages.Queries.Common;
using CleanArch.Domain.Members;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Messages.Queries.GetMessages;

public record GetMessagesQuery(MessageParams MessageParams) : QueryParamsBase, IQuery<PaginatedList<MessageDto>>;

public class GetMessagesQueryHandler(IApplicationDbContext context, ICurrentUser userContext)
    : IQueryHandler<GetMessagesQuery, PaginatedList<MessageDto>>
{
    public async Task<Result<PaginatedList<MessageDto>>> Handle(
        GetMessagesQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = request.MessageParams.MemberId ?? userContext.UserId;

        if (!userId.HasValue)
            return Result.Failure<PaginatedList<MessageDto>>(UserErrors.NotFound(Guid.Empty));

        // Get member
        var member = await context.Members.FirstOrDefaultAsync(m => m.Id == userId.Value, cancellationToken);

        if (member == null)
            return Result.Failure<PaginatedList<MessageDto>>(MemberErrors.NotFound);

        var query = context.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();

        query = request.MessageParams.Container switch
        {
            "Outbox" => query.Where(x => x.SenderId == member.Id && !x.SenderDeleted),
            _ => query.Where(x => x.RecipientId == member.Id && !x.RecipientDeleted),
        };

        return await query.ProjectToPagedAsync<MessageDto>(request);
    }
}
