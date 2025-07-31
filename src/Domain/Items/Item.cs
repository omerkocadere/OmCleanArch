using System.ComponentModel.DataAnnotations.Schema;
using CleanArch.Domain.Auctions;

namespace CleanArch.Domain.Items;

[Table("Items", Schema = "carsties")]
public class Item : BaseEntity<Guid>
{
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public required string Color { get; set; }
    public int Mileage { get; set; }
    public required string ImageUrl { get; set; }

    // navigation properties
    public Auction Auction { get; set; } = null!;
    public Guid AuctionId { get; set; }
}
