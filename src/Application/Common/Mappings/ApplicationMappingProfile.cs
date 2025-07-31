using CleanArch.Application.Auctions.CreateAuction;
using CleanArch.Application.Auctions.DTOs;
using CleanArch.Application.TodoItems.DTOs;
using CleanArch.Domain.Auctions;
using CleanArch.Domain.TodoItems;
using Contracts;
using Mapster;

namespace CleanArch.Application.Common.Mappings;

public static class MappingConfig
{
    public static void Configure()
    {
        // Global DateTimeOffset -> DateTime mapping
        TypeAdapterConfig<DateTimeOffset, DateTime>
            .NewConfig()
            .MapWith(src => src.DateTime);

        #region TodoItems

        // CreateTodoItemCommand -> TodoItem: Otomatik (property isimleri aynı)
        // TodoItem -> LookupDto: Otomatik (property isimleri aynı)

        // Sadece Priority için özel mapping gerekli (enum -> int)
        TypeAdapterConfig<TodoItem, TodoItemDto>.NewConfig().Map(dest => dest.Priority, src => (int)src.Priority);

        #endregion

        #region TodoLists

        // TodoList -> TodoListDto: Otomatik
        // TodoList -> LookupDto: Otomatik
        // TodoItem -> TodoItemDto zaten yukarıda tanımlandı

        #endregion

        #region Users

        // User -> UserDto: Otomatik
        // CreateUserCommand -> User: Otomatik

        #endregion

        #region Auctions

        // Auction -> AuctionDto: Flattening ve Status conversion
        TypeAdapterConfig<Auction, AuctionDto>.NewConfig().Map(dest => dest, src => src.Item);
        TypeAdapterConfig<Auction, AuctionCreated>.NewConfig().Map(dest => dest, src => src.Item);

        // CreateAuctionCommand -> Auction: Item property mapping
        TypeAdapterConfig<CreateAuctionCommand, Auction>
            .NewConfig()
            .Map(dest => dest.Item, src => src);

        // CreateAuctionCommand -> Item: Otomatik

        #endregion
    }
}
