using CleanArch.Domain.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Data.EntityConfigurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasOne(x => x.Recipient).WithMany(m => m.MessagesReceived).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Sender).WithMany(m => m.MessagesSent).OnDelete(DeleteBehavior.Restrict);
    }
}
