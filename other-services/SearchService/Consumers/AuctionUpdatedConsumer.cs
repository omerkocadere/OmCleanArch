using Contracts;
using Mapster;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine($"--> Auction updated: {context.Message.Id}");

        var item = context.Message.Adapt<Item>();

        var result = await DB.Update<Item>()
            .Match(a => a.ID == context.Message.Id)
            .ModifyOnly(
                x => new
                {
                    x.Color,
                    x.Make,
                    x.Model,
                    x.Mileage,
                    x.Year,
                },
                item
            )
            .ExecuteAsync();

        if (!result.IsAcknowledged)
            throw new MessageException(typeof(AuctionUpdated), "Failed to update auction");
    }
}
