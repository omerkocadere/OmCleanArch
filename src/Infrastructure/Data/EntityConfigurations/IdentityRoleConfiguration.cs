using CleanArch.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Data.EntityConfigurations;

internal sealed class IdentityRoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        // Seed default roles
        builder.HasData(
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = UserRoles.Member,
                NormalizedName = UserRoles.Member.ToUpperInvariant(),
                ConcurrencyStamp = "10000000-0000-0000-0000-000000000001",
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Name = UserRoles.Moderator,
                NormalizedName = UserRoles.Moderator.ToUpperInvariant(),
                ConcurrencyStamp = "10000000-0000-0000-0000-000000000002",
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Name = UserRoles.Admin,
                NormalizedName = UserRoles.Admin.ToUpperInvariant(),
                ConcurrencyStamp = "10000000-0000-0000-0000-000000000003",
            }
        );
    }
}
