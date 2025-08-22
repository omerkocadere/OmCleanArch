using CleanArch.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Data.EntityConfigurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .Property(u => u.Email)
            .HasConversion(email => email.Value, value => Email.Create(value).Value) // Bu runtime'da validate edilmiş olduğunu varsayıyor
            .HasMaxLength(320);

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
