using CleanArch.Application.Auctions.DTOs;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;

namespace CleanArch.Application.Auctions.GetAuctions;

public record GetAuctionsQuery(DateTime? Date = null) : IQuery<List<AuctionDto>>;

public class GetAuctionsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IQueryHandler<GetAuctionsQuery, List<AuctionDto>>
{
    public async Task<Result<List<AuctionDto>>> Handle(GetAuctionsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (request.Date.HasValue)
        {
            query = query.Where(x => x.UpdatedAt > request.Date.Value.ToUniversalTime());
        }

        var auctionDtos = await query
            .ProjectTo<AuctionDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return auctionDtos;
    }
}
