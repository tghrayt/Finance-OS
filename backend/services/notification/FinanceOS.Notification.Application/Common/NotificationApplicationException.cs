namespace FinanceOS.Notification.Application.Common;

public abstract class NotificationApplicationException(string message) : Exception(message);

public sealed class NotificationNotFoundException(string message) : NotificationApplicationException(message);
