using CleanArch.Domain.Members;
using CleanArch.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Data.EntityConfigurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        // Foreign key relationship with ApplicationUser
        // Each Member must have a corresponding ApplicationUser
        // But ApplicationUser can exist without a Member
        builder
            .HasOne<ApplicationUser>()
            .WithOne(u => u.Member)
            .HasForeignKey<Member>(m => m.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
