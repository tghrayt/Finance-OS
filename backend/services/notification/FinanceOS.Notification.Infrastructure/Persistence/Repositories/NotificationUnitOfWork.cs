using FinanceOS.Notification.Application.Abstractions;

namespace FinanceOS.Notification.Infrastructure.Persistence.Repositories;

internal sealed class NotificationUnitOfWork(NotificationDbContext dbContext) : INotificationUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken) => await dbContext.SaveChangesAsync(cancellationToken);
}
