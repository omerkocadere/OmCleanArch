using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Messages;

namespace CleanArch.Application.Messages.Commands.AddToGroup;

public class AddToGroupCommandHandler(IApplicationDbContext context) : ICommandHandler<AddToGroupCommand, bool>
{
    public async Task<Result<bool>> Handle(AddToGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await context
            .Groups.Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Name == request.GroupName, cancellationToken);

        var connection = new Connection(request.ConnectionId, request.UserId);

        if (group == null)
        {
            group = new Group(request.GroupName);
            context.Groups.Add(group);
        }

        group.Connections.Add(connection);
        var result = await context.SaveChangesAsync(cancellationToken) > 0;

        return result;
    }
}
