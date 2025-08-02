using System;
using Contracts;
using Mapster;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine($"-->  AuctionCreatedConsumer: {context.Message.Id}");

        var item = context.Message.Adapt<Item>();

        if (item.Model == "Foo")
            throw new ArgumentException("Model cannot be Foo");

        await item.SaveAsync();
    }
}
