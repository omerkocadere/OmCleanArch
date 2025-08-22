using CleanArch.Application.Auctions.DTOs;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Auctions;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Auctions.GetAuctionById;

public sealed record GetAuctionByIdQuery(Guid AuctionId) : IQuery<AuctionDto>;

public class GetAuctionByIdQueryHandler(IApplicationDbContext context) : IQueryHandler<GetAuctionByIdQuery, AuctionDto>
{
    public async Task<Result<AuctionDto>> Handle(GetAuctionByIdQuery query, CancellationToken cancellationToken)
    {
        var auction = await context
            .Auctions.Include(x => x.Item)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == query.AuctionId, cancellationToken);

        if (auction is null)
        {
            return Result.Failure<AuctionDto>(AuctionErrors.NotFound(query.AuctionId));
        }

        var auctionDto = auction.Adapt<AuctionDto>();
        return auctionDto;
    }
}
