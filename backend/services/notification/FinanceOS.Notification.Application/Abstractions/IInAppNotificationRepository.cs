using FinanceOS.Notification.Domain.Common;
using FinanceOS.Notification.Domain.InApp;

namespace FinanceOS.Notification.Application.Abstractions;

public interface IInAppNotificationRepository
{
    Task AddAsync(InAppNotification notification, CancellationToken cancellationToken);

    Task<InAppNotification?> GetByIdAsync(InAppNotificationId id, HouseholdId householdId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<InAppNotification>> ListByHouseholdAsync(HouseholdId householdId, int page, int pageSize, CancellationToken cancellationToken);
}
