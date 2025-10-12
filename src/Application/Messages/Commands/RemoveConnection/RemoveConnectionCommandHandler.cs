using CleanArch.Application.Common.Interfaces;

namespace CleanArch.Application.Messages.Commands.RemoveConnection;

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
