using CleanArch.Application.Auctions.DTOs;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Auctions;
using Contracts;
using MassTransit;

namespace CleanArch.Application.Auctions.Create;

public class CreateAuctionCommandHandler(
    IApplicationDbContext context,
    IPublishEndpoint publishEndpoint,
    ICurrentUser userContext
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
