using FinanceOS.Notification.Domain.InApp;
using Microsoft.EntityFrameworkCore;

namespace FinanceOS.Notification.Infrastructure.Persistence;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<InAppNotification> InAppNotifications => Set<InAppNotification>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notification");
        modelBuilder.ApplyConfiguration(new InAppNotificationConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
    }
}
