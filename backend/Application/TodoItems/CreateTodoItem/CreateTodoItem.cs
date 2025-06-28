using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.TodoItems;
using CleanArch.Domain.Users;
using AutoMapper;

namespace CleanArch.Application.TodoItems.CreateTodoItem;

public record CreateTodoItemCommand() : IRequest<Result<int>>
{
    public int ListId { get; init; }
    public required string Title { get; init; }
    public string? Note { get; init; }
    public PriorityLevel Priority { get; init; }
    public int UserId { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Labels { get; set; } = [];
}

public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateTodoItemCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<int>> Handle(
        CreateTodoItemCommand request,
        CancellationToken cancellationToken
    )
    {
        User? user = await _context
            .Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<int>(UserErrors.NotFound(request.UserId));
        }

        var entity = _mapper.Map<TodoItem>(request);
        entity.UserId = user.Id;

        entity.AddDomainEvent(new TodoItemCreatedEvent(entity));

        _context.TodoItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
