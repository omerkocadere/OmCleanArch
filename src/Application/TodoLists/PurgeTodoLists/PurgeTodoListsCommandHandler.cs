using CleanArch.Application.Common.Interfaces;

namespace CleanArch.Application.TodoLists.PurgeTodoLists;

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
