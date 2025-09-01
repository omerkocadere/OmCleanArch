using CleanArch.Domain.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Data.EntityConfigurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);

        builder.Property(p => p.DisplayName).IsRequired().HasMaxLength(200);

        builder.Property(p => p.Description).HasMaxLength(500);

        builder.Property(p => p.Category).HasMaxLength(100);

        // Indexes - business requirement
        builder.HasIndex(p => p.Name).IsUnique();
        builder.HasIndex(p => p.Category);

        // EF Core 9 conventions handle everything else automatically!
        // - Primary key (Id)
        // - Default values (from entity)
        // - Many-to-many relationships (from navigation properties)
    }
}
