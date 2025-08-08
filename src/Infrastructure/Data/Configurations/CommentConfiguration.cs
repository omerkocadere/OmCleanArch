using CleanArch.Domain.Comments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content).IsRequired().HasMaxLength(1000);

        builder.Property(c => c.AuthorName).IsRequired().HasMaxLength(100);

        builder.Property(c => c.Email).IsRequired().HasMaxLength(256);

        builder.Property(c => c.PostId).IsRequired();

        builder.Property(c => c.Status).HasConversion<int>().IsRequired();

        // Audit properties (inherited from BaseAuditableEntity)
        builder.Property(c => c.Created).IsRequired();

        builder.Property(c => c.CreatedBy).IsRequired(false);

        builder.Property(c => c.LastModified).IsRequired();

        builder.Property(c => c.LastModifiedBy).IsRequired(false);

        // Soft delete properties (from ISoftDeletable)
        builder.Property(c => c.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.Property(c => c.DeletedAt).IsRequired(false);

        builder.Property(c => c.DeletedBy).IsRequired(false);

        // Navigation properties
        builder.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).IsRequired(false);

        // Indexes for performance
        builder.HasIndex(c => c.PostId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.IsDeleted);
        builder.HasIndex(c => c.Created);

        // Filtered index for better performance on non-deleted comments
        builder
            .HasIndex(c => c.IsDeleted)
            .HasFilter("IsDeleted = false")
            .HasDatabaseName("IX_Comments_IsDeleted_Filtered");

        // Composite index for active comments by post
        builder
            .HasIndex(c => new { c.PostId, c.IsDeleted })
            .HasFilter("IsDeleted = false")
            .HasDatabaseName("IX_Comments_PostId_Active");
    }
}
