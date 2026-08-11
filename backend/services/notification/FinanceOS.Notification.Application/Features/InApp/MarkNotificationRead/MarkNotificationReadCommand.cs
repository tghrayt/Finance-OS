namespace FinanceOS.Notification.Application.Features.InApp.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(Guid HouseholdId, Guid NotificationId);
