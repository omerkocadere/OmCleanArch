using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Security;
using CleanArch.Domain.Common;
using CleanArch.Domain.Constants;

namespace CleanArch.Application.TodoLists.PurgeTodoLists;

public record PurgeTodoListsCommand : IRequest<Result>;

public class PurgeTodoListsCommandHandler(IApplicationDbContext context)
    : IRequestHandler<PurgeTodoListsCommand, Result>
{
    public async Task<Result> Handle(PurgeTodoListsCommand request, CancellationToken cancellationToken)
    {
        context.TodoLists.RemoveRange(context.TodoLists);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
