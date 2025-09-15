using CleanArch.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Data.EntityConfigurations;

public class MemberLikeConfiguration : IEntityTypeConfiguration<MemberLike>
{
    public void Configure(EntityTypeBuilder<MemberLike> builder)
    {
        // Composite primary key
        builder.HasKey(x => new { x.SourceMemberId, x.TargetMemberId });

        // Configure relationships
        builder
            .HasOne(x => x.SourceMember)
            .WithMany(x => x.LikedMembers)
            .HasForeignKey(x => x.SourceMemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.TargetMember)
            .WithMany(x => x.LikedByMembers)
            .HasForeignKey(x => x.TargetMemberId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
