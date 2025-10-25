using CleanArch.Application.Auctions.DTOs;
using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Auctions;
using Contracts;
using MassTransit;

namespace CleanArch.Application.Auctions.Update;

public class UpdateAuctionCommandHandler(
    IApplicationDbContext context,
    IPublishEndpoint publishEndpoint,
    ICurrentUser userContext
) : IQueryHandler<UpdateAuctionCommand, AuctionDto>
{
    public async Task<Result<AuctionDto>> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await context
            .Auctions.Include(x => x.Item)
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (auction is null)
        {
            return Result.Failure<AuctionDto>(AuctionErrors.NotFound(request.Id));
        }

        if (auction.Seller != userContext.UserName)
        {
            return Result.Failure<AuctionDto>(UserErrors.Forbidden);
        }

        // Elegant property update with null-coalescing
        auction.Item.Make = request.Make ?? auction.Item.Make;
        auction.Item.Model = request.Model ?? auction.Item.Model;
        auction.Item.Year = request.Year ?? auction.Item.Year;
        auction.Item.Color = request.Color ?? auction.Item.Color;
        auction.Item.Mileage = request.Mileage ?? auction.Item.Mileage;

        await publishEndpoint.Publish(auction.Adapt<AuctionUpdated>(), cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var auctionDto = auction.Adapt<AuctionDto>();
        return auctionDto;
    }
}
