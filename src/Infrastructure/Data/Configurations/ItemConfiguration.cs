using CleanArch.Domain.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Make).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Model).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Year).IsRequired();
        builder.Property(x => x.Color).IsRequired().HasMaxLength(30);
        builder.Property(x => x.Mileage).IsRequired();
        builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(500);

        // Navigation properties are handled in AuctionConfiguration
    }
}
