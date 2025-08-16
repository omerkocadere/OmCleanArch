using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Auctions;
using Contracts;
using MassTransit;

namespace CleanArch.Application.Auctions.Consumers;

public class AuctionFinishedConsumer(IApplicationDbContext dbContext) : IConsumer<AuctionFinished>
{
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        Console.WriteLine("--> Consuming Auction Finished");

        var auction =
            await dbContext.Auctions.FindAsync(context.Message.AuctionId)
            ?? throw new InvalidOperationException($"Auction with ID {context.Message.AuctionId} not found.");

        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = context.Message.Amount;
        }

        auction.Status = auction.SoldAmount > auction.ReservePrice ? Status.Finished : Status.ReserveNotMet;

        await dbContext.SaveChangesAsync();
    }
}
