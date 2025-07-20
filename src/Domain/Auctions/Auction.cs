using CleanArch.Domain.Items;

namespace CleanArch.Domain.Auctions;

public class Auction : BaseAuditableEntity
{
    public int ReservePrice { get; set; }
    public required string Seller { get; set; }
    public string? Winner { get; set; }
    public int? SoldAmount { get; set; }
    public int? CurrentHighBid { get; set; }
    public DateTimeOffset AuctionEnd { get; set; }
    public Status Status { get; set; }
    public required Item Item { get; set; }
}
