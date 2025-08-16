using System;
using Contracts;
using MassTransit;

namespace CleanArch.Application.Auctions.Consumers;

public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        Console.WriteLine("--> Consuming faulty creation");

        var exception = context.Message.Exceptions[0];

        if (exception.ExceptionType == typeof(ArgumentException).FullName)
        {
            context.Message.Message.Model = "FooBar";
            await context.Publish(context.Message.Message);
        }
        else
        {
            Console.WriteLine($"--> Exception: Update error dashboard somewhere");
        }
    }
}
