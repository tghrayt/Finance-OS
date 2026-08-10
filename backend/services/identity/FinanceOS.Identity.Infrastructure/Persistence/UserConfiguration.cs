using FinanceOS.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOS.Identity.Infrastructure.Persistence;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasConversion(id => id.Value, value => new UserId(value))
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(user => user.FirstName)
            .HasMaxLength(100)
            .HasColumnName("first_name")
            .IsRequired();

        builder.Property(user => user.LastName)
            .HasMaxLength(100)
            .HasColumnName("last_name")
            .IsRequired();

        builder.Property(user => user.Email)
            .HasConversion(email => email.Value, value => EmailAddress.Create(value))
            .HasMaxLength(320)
            .HasColumnName("email")
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.PreferredCurrency)
            .HasMaxLength(3)
            .HasColumnName("preferred_currency")
            .IsRequired();

        builder.Property(user => user.Language)
            .HasMaxLength(10)
            .HasColumnName("language")
            .IsRequired();

        builder.Property(user => user.TimeZone)
            .HasMaxLength(100)
            .HasColumnName("time_zone")
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
