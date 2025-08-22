using CleanArch.Domain.TodoItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Data.EntityConfigurations;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();

        // Soft delete global query filter
        builder.HasQueryFilter(t => !t.IsDeleted);

        // Indexes for performance
        builder.HasIndex(t => t.IsDeleted);
        builder.HasIndex(t => t.Created);
    }
}
