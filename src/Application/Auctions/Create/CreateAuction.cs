using CleanArch.Application.Auctions.DTOs;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Auctions;
using Contracts;
using MassTransit;

namespace CleanArch.Application.Auctions.Create;

public record CreateAuctionCommand : ICommand<AuctionDto>
{
    public required string Make { get; init; }
    public required string Model { get; init; }
    public int Year { get; init; }
    public required string Color { get; init; }
    public int Mileage { get; init; }
    public required string ImageUrl { get; init; }
    public int ReservePrice { get; init; }
    public DateTime AuctionEnd { get; init; }
}

public class CreateAuctionCommandHandler(
    IApplicationDbContext context,
    IPublishEndpoint publishEndpoint,
    IUserContext userContext
) : ICommandHandler<CreateAuctionCommand, AuctionDto>
{
    public async Task<Result<AuctionDto>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = request.Adapt<Auction>();
        auction.Seller = userContext.UserName ?? "Unknown User";

        context.Auctions.Add(auction);
        await publishEndpoint.Publish(auction.Adapt<AuctionCreated>(), cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var auctionDto = auction.Adapt<AuctionDto>();
        return auctionDto;
    }
}
