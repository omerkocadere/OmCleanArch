using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Messages.Commands.RemoveConnection;

public record RemoveConnectionCommand(string ConnectionId) : ICommand;

public class RemoveConnectionCommandHandler(IApplicationDbContext context) : ICommandHandler<RemoveConnectionCommand>
{
    public async Task<Result> Handle(RemoveConnectionCommand request, CancellationToken cancellationToken)
    {
        await context
            .Connections.Where(x => x.ConnectionId == request.ConnectionId)
            .ExecuteDeleteAsync(cancellationToken);
        return Result.Success();
    }
}
