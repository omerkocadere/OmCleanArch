using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Members.Commands.UpdateUserActivity;

public record UpdateUserActivityCommand : ICommand;

public class UpdateUserActivityCommandHandler(IApplicationDbContext context, IUserContext userContext)
    : ICommandHandler<UpdateUserActivityCommand>
{
    public async Task<Result> Handle(UpdateUserActivityCommand request, CancellationToken cancellationToken)
    {
        if (userContext.UserId is null)
        {
            return Result.Success(); // No authenticated user, skip update
        }

        await context
            .Members.Where(m => m.Id == userContext.UserId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(m => m.LastActive, DateTime.UtcNow), cancellationToken);

        return Result.Success();
    }
}
