using CleanArch.Application.Common.Interfaces;
using Contracts;
using MassTransit;

namespace CleanArch.Application.Auctions.Consumers;

public class BidPlacedConsumer(IApplicationDbContext dbContext) : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("--> Consuming Bid Placed");

        var auction = await dbContext.Auctions.FindAsync(context.Message.AuctionId);

        if (
            auction != null
            && (
                auction.CurrentHighBid == null
                || context.Message.BidStatus.Contains("Accepted") && context.Message.Amount > auction.CurrentHighBid
            )
        )
        {
            auction.CurrentHighBid = context.Message.Amount;
            await dbContext.SaveChangesAsync();
        }
    }
}
