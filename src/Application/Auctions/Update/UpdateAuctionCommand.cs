using CleanArch.Application.Auctions.DTOs;

namespace CleanArch.Application.Auctions.Update;

public record UpdateAuctionCommand : IQuery<AuctionDto>
{
    public Guid Id { get; init; }
    public string? Make { get; init; }
    public string? Model { get; init; }
    public int? Year { get; init; }
    public string? Color { get; init; }
    public int? Mileage { get; init; }
}
