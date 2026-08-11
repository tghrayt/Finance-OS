using FinanceOS.Notification.Domain.Common;
using FinanceOS.Notification.Domain.InApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOS.Notification.Infrastructure.Persistence;

internal sealed class InAppNotificationConfiguration : IEntityTypeConfiguration<InAppNotification>
{
    public void Configure(EntityTypeBuilder<InAppNotification> builder)
    {
        builder.ToTable("in_app_notifications");
        builder.HasKey(notification => notification.Id);
        builder.Property(notification => notification.Id).HasConversion(id => id.Value, value => new InAppNotificationId(value)).ValueGeneratedNever().HasColumnName("id");
        builder.Property(notification => notification.HouseholdId).HasConversion(id => id.Value, value => new HouseholdId(value)).HasColumnName("household_id").IsRequired();
        builder.Property(notification => notification.Type).HasMaxLength(120).HasColumnName("type").IsRequired();
        builder.Property(notification => notification.Title).HasMaxLength(220).HasColumnName("title").IsRequired();
        builder.Property(notification => notification.Body).HasMaxLength(1000).HasColumnName("body").IsRequired();
        builder.Property(notification => notification.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(notification => notification.ReadAt).HasColumnName("read_at");
        builder.HasIndex(notification => new { notification.HouseholdId, notification.CreatedAt });
    }
}
