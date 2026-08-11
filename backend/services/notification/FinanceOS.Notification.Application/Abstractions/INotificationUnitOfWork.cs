namespace FinanceOS.Notification.Application.Abstractions;

public interface INotificationUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
