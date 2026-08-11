namespace FinanceOS.Notification.Domain.InApp;

public readonly record struct InAppNotificationId(Guid Value)
{
    public static InAppNotificationId New() => new(Guid.NewGuid());
}
