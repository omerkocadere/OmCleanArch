using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;
using CleanArch.Domain.Messages;

namespace CleanArch.Application.Messages.Queries.GetMessageGroup;

public record GetMessageGroupQuery(string GroupName) : IQuery<Group?>;

public class GetMessageGroupQueryHandler(IApplicationDbContext context) : IQueryHandler<GetMessageGroupQuery, Group?>
{
    public async Task<Result<Group?>> Handle(GetMessageGroupQuery request, CancellationToken cancellationToken)
    {
        var group = await context
            .Groups.Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Name == request.GroupName, cancellationToken);

        return group;
    }
}
