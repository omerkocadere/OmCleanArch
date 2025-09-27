using CleanArch.Application.Auctions.DTOs;
using CleanArch.Application.Common.Interfaces;

namespace CleanArch.Application.Auctions.Get;

public record GetAuctionsQuery(string? Date) : IQuery<List<AuctionDto>>;

public class GetAuctionsQueryHandler(IApplicationDbContext context) : IQueryHandler<GetAuctionsQuery, List<AuctionDto>>
{
    public async Task<Result<List<AuctionDto>>> Handle(GetAuctionsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(request.Date))
        {
            query = query.Where(x => x.LastModified.CompareTo(DateTime.Parse(request.Date).ToUniversalTime()) > 0);
        }

        var auctionDtos = await query.ProjectToType<AuctionDto>().ToListAsync(cancellationToken);

        return auctionDtos;
    }
}
