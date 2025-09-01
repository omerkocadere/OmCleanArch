using CleanArch.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Data.EntityConfigurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);

        builder.Property(r => r.NormalizedName).IsRequired().HasMaxLength(100);

        builder.Property(r => r.Description).HasMaxLength(500);

        // Indexes - business requirement
        builder.HasIndex(r => r.Name).IsUnique();
        builder.HasIndex(r => r.NormalizedName).IsUnique();

        // EF Core 9 conventions handle everything else automatically!
        // - Primary key (Id)
        // - Default values (from entity)
        // - Many-to-many relationships (from navigation properties)
        // - Foreign keys, composite keys, cascade delete
    }
}
