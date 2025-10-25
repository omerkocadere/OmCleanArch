using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Common.Security;
using CleanArch.Domain.Common;
using CleanArch.Domain.TodoItems;
using CleanArch.Domain.Users;

namespace CleanArch.Application.TodoLists.GetTodos;

public record GetTodosQuery(Guid UserId) : IRequest<Result<TodosVm>>;

public class GetTodosQueryHandler(IApplicationDbContext context, ICurrentUser userContext)
    : IRequestHandler<GetTodosQuery, Result<TodosVm>>
{
    public async Task<Result<TodosVm>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<TodosVm>(UserErrors.Unauthorized);
        }

        var result = new TodosVm
        {
            PriorityLevels =
            [
                .. Enum.GetValues<PriorityLevel>().Select(p => new LookupDto { Id = (int)p, Title = p.ToString() }),
            ],

            Lists = await context
                .TodoLists.AsNoTracking()
                .Where(l => l.UserId == request.UserId)
                .ProjectToType<TodoListDto>()
                .OrderBy(t => t.Title)
                .ToListAsync(cancellationToken),
        };

        return result;
    }
}
