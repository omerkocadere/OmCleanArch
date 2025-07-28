using AutoMapper;
using CleanArch.Application.Auctions.CreateAuction;
using CleanArch.Application.Auctions.DTOs;
using CleanArch.Application.Common.Models;
using CleanArch.Application.TodoItems.CreateTodoItem;
using CleanArch.Application.TodoItems.DTOs;
using CleanArch.Application.TodoLists.GetTodos;
using CleanArch.Application.Users.Create;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Auctions;
using CleanArch.Domain.Items;
using CleanArch.Domain.TodoItems;
using CleanArch.Domain.TodoLists;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Common.Mappings;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        #region TodoItems

        CreateMap<CreateTodoItemCommand, TodoItem>();
        CreateMap<TodoItem, TodoItemDto>().ForMember(d => d.Priority, opt => opt.MapFrom(s => (int)s.Priority));
        CreateMap<TodoItem, LookupDto>();

        #endregion

        #region TodoLists

        CreateMap<TodoList, TodoListDto>();
        CreateMap<TodoItem, TodoItemDto>().ForMember(d => d.Priority, opt => opt.MapFrom(s => (int)s.Priority));
        CreateMap<TodoList, LookupDto>();

        #endregion

        #region Users

        CreateMap<User, UserDto>();
        CreateMap<CreateUserCommand, User>();

        #endregion

        #region Auctions

        CreateMap<Auction, AuctionDto>()
            .IncludeMembers(x => x.Item)
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
        CreateMap<Item, AuctionDto>();
        CreateMap<CreateAuctionCommand, Auction>().ForMember(dest => dest.Item, opt => opt.MapFrom(src => src));
        CreateMap<CreateAuctionCommand, Item>();

        #endregion
    }
}
