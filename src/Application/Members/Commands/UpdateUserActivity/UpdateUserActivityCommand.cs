using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Members.Commands.UpdateUserActivity;

public record UpdateUserActivityCommand(Guid UserId) : ICommand;

public class UpdateUserActivityCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateUserActivityCommand>
{
    public async Task<Result> Handle(UpdateUserActivityCommand request, CancellationToken cancellationToken)
    {
        await context
            .Members.Where(m => m.Id == request.UserId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(m => m.LastActive, DateTime.UtcNow), cancellationToken);

        return Result.Success();
    }
}
