using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Common.Security;
using CleanArch.Domain.TodoItems;

namespace CleanArch.Application.TodoLists.GetTodos;

[Authorize]
public record GetTodosQuery(Guid UserId) : IRequest<Result<TodosVm>>;

public class GetTodosQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetTodosQuery, Result<TodosVm>>
{
    public async Task<Result<TodosVm>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        var result = new TodosVm
        {
            PriorityLevels =
            [
                .. Enum.GetValues<PriorityLevel>().Select(p => new LookupDto { Id = (int)p, Title = p.ToString() }),
            ],

            Lists = await context
                .TodoLists.AsNoTracking()
                .Where(l => l.UserId == request.UserId)
                .ProjectTo<TodoListDto>(mapper.ConfigurationProvider)
                .OrderBy(t => t.Title)
                .ToListAsync(cancellationToken),
        };

        return result;
    }
}
