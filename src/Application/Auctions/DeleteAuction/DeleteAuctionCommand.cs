using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.Auctions;
using CleanArch.Domain.Users;
using Contracts;
using MassTransit;

namespace CleanArch.Application.Auctions.DeleteAuction;

public record DeleteAuctionCommand(Guid Id) : ICommand;

public class DeleteAuctionCommandHandler(
    IApplicationDbContext context,
    IPublishEndpoint publishEndpoint,
    IUserContext userContext
) : ICommandHandler<DeleteAuctionCommand>
{
    public async Task<Result> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await context.Auctions.SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (auction is null)
        {
            return Result.Failure(AuctionErrors.NotFound(request.Id));
        }

        if (auction.Seller != userContext.UserName)
        {
            return Result.Failure(UserErrors.Forbidden);
        }

        context.Auctions.Remove(auction);
        await publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() }, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
