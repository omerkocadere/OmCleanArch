using CleanArch.Application.Auctions.DTOs;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.Auctions;

namespace CleanArch.Application.Auctions.GetAuctionById;

public sealed record GetAuctionByIdQuery(Guid AuctionId) : IQuery<AuctionDto>;

public class GetAuctionByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IQueryHandler<GetAuctionByIdQuery, AuctionDto>
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

        var auctionDto = mapper.Map<AuctionDto>(auction);
        return auctionDto;
    }
}
