using CleanArch.Application.Common.Models;
using CleanArch.Application.TodoItems.CreateTodoItem;
using CleanArch.Application.TodoLists.GetTodos;
using CleanArch.Domain.TodoItems;
using CleanArch.Domain.TodoLists;

namespace CleanArch.Application.Common.Mappings;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        #region TodoItems

        CreateMap<CreateTodoItemCommand, TodoItem>();
        CreateMap<TodoItem, TodoItems.DTOs.TodoItemDto>()
            .ForMember(d => d.Priority, opt => opt.MapFrom(s => (int)s.Priority));
        CreateMap<TodoItem, LookupDto>();

        #endregion

        #region TodoLists

        CreateMap<TodoList, TodoListDto>();
        CreateMap<TodoItem, TodoItemDto>()
            .ForMember(d => d.Priority, opt => opt.MapFrom(s => (int)s.Priority));
        CreateMap<TodoList, LookupDto>();

        #endregion
    }
}
