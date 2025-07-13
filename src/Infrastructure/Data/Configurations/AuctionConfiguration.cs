using CleanArch.Domain.Auctions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Data.Configurations;

public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        // builder.HasKey(x => x.Id);
        // builder.Property(x => x.Seller).IsRequired().HasMaxLength(100);
        // builder.Property(x => x.Winner).HasMaxLength(100);
        // builder.Property(x => x.ReservePrice).IsRequired();
        // builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        // builder.Property(x => x.CreatedAt).IsRequired();
        // builder.Property(x => x.UpdatedAt).IsRequired();
        // builder.Property(x => x.AuctionEnd).IsRequired();

        // // Navigation properties
        // builder
        //     .HasOne(x => x.Item)
        //     .WithOne(x => x.Auction)
        //     .HasForeignKey<CleanArch.Domain.Items.Item>(x => x.AuctionId)
        //     .OnDelete(DeleteBehavior.Cascade);
    }
}
