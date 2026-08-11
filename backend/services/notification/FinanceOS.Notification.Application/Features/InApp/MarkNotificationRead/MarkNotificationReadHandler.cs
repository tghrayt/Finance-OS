using FinanceOS.Notification.Application.Abstractions;
using FinanceOS.Notification.Application.Common;
using FinanceOS.Notification.Application.Features.InApp;
using FinanceOS.Notification.Domain.Common;
using FinanceOS.Notification.Domain.InApp;

namespace FinanceOS.Notification.Application.Features.InApp.MarkNotificationRead;

public sealed class MarkNotificationReadHandler(IInAppNotificationRepository notifications, INotificationUnitOfWork unitOfWork)
{
    public async Task<InAppNotificationResult> HandleAsync(MarkNotificationReadCommand command, CancellationToken cancellationToken)
    {
        var notification = await notifications.GetByIdAsync(
                new InAppNotificationId(command.NotificationId),
                new HouseholdId(command.HouseholdId),
                cancellationToken)
            ?? throw new NotificationNotFoundException("Notification was not found.");

        notification.MarkRead(DateTimeOffset.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return InAppNotificationResult.FromNotification(notification);
    }
}
