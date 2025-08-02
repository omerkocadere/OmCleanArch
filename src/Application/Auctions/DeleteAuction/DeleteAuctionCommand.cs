using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.Auctions;
using MassTransit;

namespace CleanArch.Application.Auctions.DeleteAuction;

public record DeleteAuctionCommand(Guid Id) : ICommand;

public class DeleteAuctionCommandHandler(IApplicationDbContext context, IPublishEndpoint publishEndpoint)
    : ICommandHandler<DeleteAuctionCommand>
{
    public async Task<Result> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await context.Auctions.SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (auction is null)
        {
            return Result.Failure(AuctionErrors.NotFound(request.Id));
        }

        // TODO: Check if seller is the same as current user

        context.Auctions.Remove(auction);
        await publishEndpoint.Publish(new { Id = auction.Id.ToString() }, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
