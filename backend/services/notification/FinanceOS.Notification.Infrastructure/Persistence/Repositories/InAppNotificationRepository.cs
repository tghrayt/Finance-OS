using FinanceOS.Notification.Application.Abstractions;
using FinanceOS.Notification.Domain.Common;
using FinanceOS.Notification.Domain.InApp;
using Microsoft.EntityFrameworkCore;

namespace FinanceOS.Notification.Infrastructure.Persistence.Repositories;

internal sealed class InAppNotificationRepository(NotificationDbContext dbContext) : IInAppNotificationRepository
{
    public async Task AddAsync(InAppNotification notification, CancellationToken cancellationToken) =>
        await dbContext.InAppNotifications.AddAsync(notification, cancellationToken);

    public async Task<InAppNotification?> GetByIdAsync(InAppNotificationId id, HouseholdId householdId, CancellationToken cancellationToken) =>
        await dbContext.InAppNotifications.FirstOrDefaultAsync(
            notification => notification.Id == id && notification.HouseholdId == householdId,
            cancellationToken);

    public async Task<IReadOnlyCollection<InAppNotification>> ListByHouseholdAsync(HouseholdId householdId, int page, int pageSize, CancellationToken cancellationToken) =>
        await dbContext.InAppNotifications
            .AsNoTracking()
            .Where(notification => notification.HouseholdId == householdId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
}
