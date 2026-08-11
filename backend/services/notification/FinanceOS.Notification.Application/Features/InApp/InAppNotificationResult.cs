using FinanceOS.Notification.Domain.InApp;

namespace FinanceOS.Notification.Application.Features.InApp;

public sealed record InAppNotificationResult(
    Guid NotificationId,
    Guid HouseholdId,
    string Type,
    string Title,
    string Body,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt)
{
    public static InAppNotificationResult FromNotification(InAppNotification notification) =>
        new(
            notification.Id.Value,
            notification.HouseholdId.Value,
            notification.Type,
            notification.Title,
            notification.Body,
            notification.CreatedAt,
            notification.ReadAt);
}
