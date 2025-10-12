using CleanArch.Application.Auctions.DTOs;

namespace CleanArch.Application.Auctions.Create;

public record CreateAuctionCommand : ICommand<AuctionDto>
{
    public required string Make { get; init; }
    public required string Model { get; init; }
    public int Year { get; init; }
    public required string Color { get; init; }
    public int Mileage { get; init; }
    public required string ImageUrl { get; init; }
    public int ReservePrice { get; init; }
    public DateTime AuctionEnd { get; init; }
}
