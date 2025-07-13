using CleanArch.Application.Auctions.DTOs;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.Auctions;

namespace CleanArch.Application.Auctions.UpdateAuction;

public record UpdateAuctionCommand : IQuery<AuctionDto>
{
    public Guid Id { get; init; }
    public string? Make { get; init; }
    public string? Model { get; init; }
    public int? Year { get; init; }
    public string? Color { get; init; }
    public int? Mileage { get; init; }
}

public class UpdateAuctionCommandHandler(IApplicationDbContext context, IMapper mapper)
    : IQueryHandler<UpdateAuctionCommand, AuctionDto>
{
    public async Task<Result<AuctionDto>> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await context
            .Auctions.Include(x => x.Item)
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (auction is null)
        {
            return Result.Failure<AuctionDto>(AuctionErrors.NotFound(request.Id));
        }

        // TODO: Check if seller is the same as current user

        // Elegant property update with null-coalescing
        auction.Item.Make = request.Make ?? auction.Item.Make;
        auction.Item.Model = request.Model ?? auction.Item.Model;
        auction.Item.Year = request.Year ?? auction.Item.Year;
        auction.Item.Color = request.Color ?? auction.Item.Color;
        auction.Item.Mileage = request.Mileage ?? auction.Item.Mileage;
        auction.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        var auctionDto = mapper.Map<AuctionDto>(auction);
        return auctionDto;
    }
}
