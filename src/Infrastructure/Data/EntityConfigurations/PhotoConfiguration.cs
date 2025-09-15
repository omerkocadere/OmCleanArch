using CleanArch.Domain.Photos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Data.EntityConfigurations;

public class PhotoConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        // Configure global query filter for Photo approval
        // This automatically excludes unapproved photos from all queries
        // Use IgnoreQueryFilters() to include unapproved photos when needed (e.g., admin operations)
        builder.HasQueryFilter(x => x.IsApproved);

        // Other Photo-specific configurations can be added here
        builder.Property(p => p.Url).IsRequired().HasMaxLength(500);

        builder.Property(p => p.PublicId).HasMaxLength(100);

        builder.HasIndex(p => p.PublicId).IsUnique();
    }
}
