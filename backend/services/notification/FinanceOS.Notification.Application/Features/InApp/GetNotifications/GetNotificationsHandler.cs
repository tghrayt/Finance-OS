using FinanceOS.Notification.Application.Abstractions;
using FinanceOS.Notification.Application.Features.InApp;
using FinanceOS.Notification.Domain.Common;

namespace FinanceOS.Notification.Application.Features.InApp.GetNotifications;

public sealed class GetNotificationsHandler(IInAppNotificationRepository notifications)
{
    public async Task<IReadOnlyCollection<InAppNotificationResult>> HandleAsync(Guid householdId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 100);
        var result = await notifications.ListByHouseholdAsync(new HouseholdId(householdId), safePage, safePageSize, cancellationToken);
        return result.Select(InAppNotificationResult.FromNotification).ToArray();
    }
}
