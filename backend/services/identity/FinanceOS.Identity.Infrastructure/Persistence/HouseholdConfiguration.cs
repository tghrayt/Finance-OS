using FinanceOS.Identity.Domain.Households;
using FinanceOS.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOS.Identity.Infrastructure.Persistence;

internal sealed class HouseholdConfiguration : IEntityTypeConfiguration<Household>
{
    public void Configure(EntityTypeBuilder<Household> builder)
    {
        builder.ToTable("households");
        builder.HasKey(household => household.Id);

        builder.Property(household => household.Id)
            .HasConversion(id => id.Value, value => new HouseholdId(value))
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(household => household.Name)
            .HasMaxLength(160)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(household => household.Currency)
            .HasMaxLength(3)
            .HasColumnName("currency")
            .IsRequired();

        builder.Property(household => household.OwnerId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(household => household.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(household => household.OwnerId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(household => household.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(household => household.Memberships)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(household => household.Memberships, membershipBuilder =>
        {
            membershipBuilder.ToTable("household_memberships");

            membershipBuilder.WithOwner()
                .HasForeignKey("household_id");

            membershipBuilder.Property<HouseholdId>("household_id")
                .HasConversion(id => id.Value, value => new HouseholdId(value));

            membershipBuilder.Property(membership => membership.UserId)
                .HasConversion(id => id.Value, value => new UserId(value))
                .HasColumnName("user_id")
                .IsRequired();

            membershipBuilder.Property(membership => membership.Role)
                .HasConversion<string>()
                .HasMaxLength(24)
                .HasColumnName("role")
                .IsRequired();

            membershipBuilder.Property(membership => membership.JoinedAt)
                .HasColumnName("joined_at")
                .IsRequired();

            membershipBuilder.HasKey("household_id", nameof(HouseholdMembership.UserId));
            membershipBuilder.HasIndex(nameof(HouseholdMembership.UserId));

            membershipBuilder.HasOne<User>()
                .WithMany()
                .HasForeignKey(membership => membership.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
