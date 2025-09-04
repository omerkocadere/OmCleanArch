using CleanArch.Application.Auctions.Create;
using CleanArch.Application.Auctions.DTOs;
using CleanArch.Application.Messages.Queries.Common;
using CleanArch.Application.TodoItems.DTOs;
using CleanArch.Application.Users.Create;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Auctions;
using CleanArch.Domain.Members;
using CleanArch.Domain.Messages;
using CleanArch.Domain.TodoItems;
using CleanArch.Domain.Users;
using Contracts;
using Mapster;

namespace CleanArch.Application.Common.Mappings;

public static class MappingConfig
{
    public static void Configure()
    {
        // Global DateTimeOffset -> DateTime mapping
        TypeAdapterConfig<DateTimeOffset, DateTime>.NewConfig().MapWith(src => src.DateTime);

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

        TypeAdapterConfig<CreateUserCommand, User>.NewConfig().Ignore(dest => dest.Email);

        // User -> UserDto: Map Email value object to string
        TypeAdapterConfig<User, UserDto>.NewConfig()
            .Map(dest => dest.Email, src => src.Email.Value);

        #endregion

        #region Auctions

        // Auction -> AuctionDto: Flattening ve Status conversion
        TypeAdapterConfig<Auction, AuctionDto>.NewConfig().Map(dest => dest, src => src.Item);
        TypeAdapterConfig<Auction, AuctionCreated>.NewConfig().Map(dest => dest, src => src.Item);
        TypeAdapterConfig<Auction, AuctionUpdated>.NewConfig().Map(dest => dest, src => src.Item);

        // CreateAuctionCommand -> Auction: Item property mapping
        TypeAdapterConfig<CreateAuctionCommand, Auction>.NewConfig().Map(dest => dest.Item, src => src);

        // CreateAuctionCommand -> Item: Otomatik

        #endregion

        #region Members

        // Member -> MemberDto: Otomatik (property isimleri aynı)
        // Photo -> PhotoDto: Otomatik (property isimleri aynı)

        #endregion

        #region Messages

        // Message -> MessageDto: Map navigation properties
        TypeAdapterConfig<Message, MessageDto>
            .NewConfig()
            .Map(dest => dest.SenderDisplayName, src => src.Sender.DisplayName)
            .Map(dest => dest.SenderImageUrl, src => src.Sender.ImageUrl ?? string.Empty)
            .Map(dest => dest.RecipientDisplayName, src => src.Recipient.DisplayName)
            .Map(dest => dest.RecipientImageUrl, src => src.Recipient.ImageUrl ?? string.Empty);

        #endregion
    }
}
